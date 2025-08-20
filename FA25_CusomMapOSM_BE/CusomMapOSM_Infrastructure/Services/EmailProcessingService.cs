using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Commons.Constant;

namespace CusomMapOSM_Infrastructure.Services;

public class EmailProcessingService : BackgroundService
{
    private readonly ILogger<EmailProcessingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqService _rabbitMqService;

    public EmailProcessingService(IServiceScopeFactory scopeFactory, ILogger<EmailProcessingService> logger, RabbitMqService rabbitMqService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _rabbitMqService = rabbitMqService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var connection = _rabbitMqService.CreateConnection();
        using var channel = _rabbitMqService.CreateEmailChannel(connection);

        // QoS: one-at-a-time processing per consumer
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        StartConsuming(channel, stoppingToken);

        // run retry loop
        _ = RetryFailedEmailsPeriodically(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            await MonitorEmailStatsAsync();
        }
    }

    private void StartConsuming(IModel channel, CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var hangfireService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            try
            {
                var json = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                var mailRequest = JsonConvert.DeserializeObject<MailRequest>(json);
                if (mailRequest != null)
                {
                    hangfireService.EnqueueEmail(mailRequest);
                    _logger.LogInformation("Queued email to Hangfire (email queue): {Email}", mailRequest.ToEmail);
                }
                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message from queue");
                channel.BasicNack(ea.DeliveryTag, false, false); // Send to DLQ
            }
        };

        channel.BasicConsume(queue: RabbitMQConstant.EmailQueue, autoAck: false, consumer: consumer);
        _logger.LogInformation("Started consuming RabbitMQ queue");
    }

    private async Task RetryFailedEmailsPeriodically(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var failedEmailStorage = scope.ServiceProvider.GetRequiredService<FailedEmailStorageService>();
                var hangfireService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

                var failedEmails = await failedEmailStorage.GetPendingFailedEmailsAsync();
                foreach (var email in failedEmails)
                {
                    try
                    {
                        var mailRequest = JsonConvert.DeserializeObject<MailRequest>(email.EmailData);
                        if (mailRequest != null)
                        {
                            hangfireService.EnqueueEmail(mailRequest);
                            await failedEmailStorage.MarkEmailAsProcessedAsync(email.Id);
                            _logger.LogInformation("Retried failed email to {Email}", mailRequest.ToEmail);
                        }
                    }
                    catch (Exception ex)
                    {
                        await failedEmailStorage.IncrementRetryCountAsync(email.Id);
                        _logger.LogError(ex, "Retry failed for {Email}", email.ToEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during failed email retry");
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task MonitorEmailStatsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var failedEmailStorage = scope.ServiceProvider.GetRequiredService<FailedEmailStorageService>();

        var failed = await failedEmailStorage.CountByStatusAsync("Failed");
        var pending = await failedEmailStorage.CountByStatusAsync("Pending");

        _logger.LogInformation("[Monitor] Failed: {Failed}, Pending: {Pending}", failed, pending);
    }
}
