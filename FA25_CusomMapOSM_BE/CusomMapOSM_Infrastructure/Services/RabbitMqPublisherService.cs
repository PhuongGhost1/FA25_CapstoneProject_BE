using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.IO;
using System.Text;
using CusomMapOSM_Commons.Constant;

namespace CusomMapOSM_Infrastructure.Services;

public class RabbitMqPublisherService : IRabbitMQService
{
    private readonly RabbitMqService _rabbitMqService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RabbitMqPublisherService> _logger;

    public RabbitMqPublisherService(RabbitMqService rabbitMqService, IServiceScopeFactory serviceScopeFactory, ILogger<RabbitMqPublisherService> logger)
    {
        _rabbitMqService = rabbitMqService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task EnqueueEmailAsync(MailRequest mailRequest)
    {
        try
        {
            using var connection = _rabbitMqService.CreateConnection();
            using var channel = _rabbitMqService.CreateEmailChannel(connection);

            // Ensure publisher confirmations are enabled
            channel.ConfirmSelect();

            var messageJson = JsonConvert.SerializeObject(mailRequest);
            var body = Encoding.UTF8.GetBytes(messageJson);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = 2; // Persistent

            var retryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<AlreadyClosedException>()
                .Or<IOException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Publish retry {Retry}/{Max} in {Delay}s for {Email}", retryCount, 3, timespan.TotalSeconds, mailRequest.ToEmail);
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                // metadata headers
                properties.Headers ??= new Dictionary<string, object>();
                properties.Headers["x-route"] = "rabbitmq";
                properties.Headers["x-created-at-utc"] = DateTime.UtcNow.ToString("o");
                properties.Headers["x-origin"] = "publisher";

                channel.BasicPublish(
                    exchange: RabbitMQConstant.EmailExchange,
                    routingKey: "email",
                    basicProperties: properties,
                    body: body
                );

                if (!channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
                {
                    throw new Exception("RabbitMQ publish was not confirmed by broker");
                }

                _logger.LogInformation("Published email message to RabbitMQ for {Email}", mailRequest.ToEmail);
                await Task.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email to RabbitMQ for {Email}. Falling back.", mailRequest.ToEmail);

            // Fallback 1: enqueue to Hangfire (fallback queue)
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var hangfireService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();
                var jobId = hangfireService.EnqueueEmailFallback(mailRequest);
                _logger.LogInformation("Fallback to Hangfire succeeded. JobId={JobId} for {Email}", jobId, mailRequest.ToEmail);
                return;
            }
            catch (Exception hangfireEx)
            {
                _logger.LogError(hangfireEx, "Fallback to Hangfire failed for {Email}. Will store to DB.", mailRequest.ToEmail);
            }

            // Fallback 2: store to DB for later retry
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var storage = scope.ServiceProvider.GetRequiredService<FailedEmailStorageService>();
                await storage.StoreFailedEmailAsync(mailRequest, $"RabbitMQ publish failed and Hangfire fallback failed: {ex.Message}");
                _logger.LogInformation("Stored failed email to DB for later retry: {Email}", mailRequest.ToEmail);
            }
            catch (Exception storageEx)
            {
                _logger.LogError(storageEx, "Failed to store failed email to DB for {Email}", mailRequest.ToEmail);
            }
        }
    }
}