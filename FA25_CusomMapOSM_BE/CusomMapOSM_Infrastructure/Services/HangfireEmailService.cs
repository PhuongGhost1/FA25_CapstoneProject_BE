using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using Microsoft.Extensions.Logging;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CusomMapOSM_Infrastructure.Services;

public class HangfireEmailService
{
    private readonly ILogger<HangfireEmailService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HangfireEmailService(IServiceProvider serviceProvider, ILogger<HangfireEmailService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string EnqueueEmail(MailRequest mailRequest)
    {
        var jobId = BackgroundJob.Enqueue(() => SendEmailWithRetryAsync(mailRequest));
        _logger.LogInformation("Email job queued with ID: {JobId} for {Email}", jobId, mailRequest.ToEmail);
        return jobId;
    }

    public string ScheduleEmail(MailRequest mailRequest, TimeSpan delay)
    {
        var jobId = BackgroundJob.Schedule(() => SendEmailWithRetryAsync(mailRequest), delay);
        _logger.LogInformation("Email job scheduled with ID: {JobId} for {Email} in {Delay}", jobId, mailRequest.ToEmail, delay);
        return jobId;
    }

    public string EnqueueRecurringEmail(MailRequest mailRequest, string cronExpression)
    {
        var jobId = $"email-{mailRequest.ToEmail}-{DateTime.UtcNow.Ticks}";
        RecurringJob.AddOrUpdate(jobId, () => SendEmailWithRetryAsync(mailRequest), cronExpression);
        _logger.LogInformation("Recurring email job created with ID: {JobId} for {Email}", jobId, mailRequest.ToEmail);
        return jobId;
    }

    [Queue("email")]
    [AutomaticRetry(Attempts = 3)]
    public async Task SendEmailWithRetryAsync(MailRequest mailRequest)
    {
        try
        {
            _logger.LogInformation("Starting email job for {Email}", mailRequest.ToEmail);

            using var scope = _serviceProvider.CreateScope();
            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

            await mailService.SendEmailAsync(mailRequest);

            _logger.LogInformation("Email sent successfully for {Email}", mailRequest.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for {Email}. Job will be retried automatically.", mailRequest.ToEmail);
            throw; // Let Hangfire handle retry
        }
    }


    public void DeleteJob(string jobId)
    {
        BackgroundJob.Delete(jobId);
        _logger.LogInformation("Email job deleted: {JobId}", jobId);
    }

    public void DeleteRecurringJob(string jobId)
    {
        RecurringJob.RemoveIfExists(jobId);
        _logger.LogInformation("Recurring email job deleted: {JobId}", jobId);
    }
}
