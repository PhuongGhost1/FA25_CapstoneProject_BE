using CusomMapOSM_Application.Models.DTOs.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Services;

public class FailedEmailStorageService
{
    private readonly CustomMapOSMDbContext _dbContext;
    private readonly ILogger<FailedEmailStorageService> _logger;

    public FailedEmailStorageService(CustomMapOSMDbContext dbContext, ILogger<FailedEmailStorageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task StoreFailedEmailAsync(MailRequest mailRequest, string failureReason)
    {
        try
        {
            var failedEmail = new FailedEmail
            {
                ToEmail = mailRequest.ToEmail,
                Subject = mailRequest.Subject,
                Body = mailRequest.Body,
                EmailData = JsonConvert.SerializeObject(mailRequest),
                FailureReason = failureReason,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                Status = FailedEmailStatus.Pending
            };

            _dbContext.FailedEmails.Add(failedEmail);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Failed email stored in database for {Email}", mailRequest.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store failed email in database for {Email}", mailRequest.ToEmail);
        }
    }

    public async Task<List<FailedEmail>> GetPendingFailedEmailsAsync(int maxRetries = 3)
    {
        return await _dbContext.FailedEmails
            .Where(fe => fe.Status == FailedEmailStatus.Pending && fe.RetryCount < maxRetries)
            .OrderBy(fe => fe.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkEmailAsProcessedAsync(int failedEmailId)
    {
        var failedEmail = await _dbContext.FailedEmails.FindAsync(failedEmailId);
        if (failedEmail != null)
        {
            failedEmail.Status = FailedEmailStatus.Processed;
            failedEmail.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task IncrementRetryCountAsync(int failedEmailId)
    {
        var failedEmail = await _dbContext.FailedEmails.FindAsync(failedEmailId);
        if (failedEmail != null)
        {
            failedEmail.RetryCount++;
            failedEmail.LastRetryAt = DateTime.UtcNow;
            
            if (failedEmail.RetryCount >= 3)
            {
                failedEmail.Status = FailedEmailStatus.Failed;
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task<int> CountByStatusAsync(string status)
    {
        return await _dbContext.FailedEmails
            .CountAsync(fe => fe.Status.ToString() == status);
    }
    public async Task<int> CountByStatusAsync(FailedEmailStatus status)
    {
        return await _dbContext.FailedEmails
            .CountAsync(fe => fe.Status == status);
    }

}

// Entity for storing failed emails
public class FailedEmail
{
    public int Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string EmailData { get; set; } = string.Empty; // Full serialized MailRequest
    public string FailureReason { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public FailedEmailStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public enum FailedEmailStatus
{
    Pending,
    Processed,
    Failed
}
