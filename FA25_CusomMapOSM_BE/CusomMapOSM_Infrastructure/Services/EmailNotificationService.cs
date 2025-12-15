using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.Templates.Email;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Services;

public interface IEmailNotificationService
{
    Task SendTransactionCompletedNotificationAsync(string userEmail, string userName, decimal amount, string planName);
    Task SendMembershipExpirationWarningAsync(string userEmail, string userName, int daysRemaining, string planName);
    Task SendMembershipExpiredNotificationAsync(string userEmail, string userName, string planName);
    Task SendQuotaExceededNotificationAsync(string userEmail, string userName, string quotaType, int currentUsage, int limit);
    Task SendQuotaWarningNotificationAsync(string userEmail, string userName, string quotaType, int currentUsage, int limit, int percentageUsed);
    Task SendExportCompletedNotificationAsync(string userEmail, string userName, string fileName, string fileSize, string downloadLink);
    Task SendExportFailedNotificationAsync(string userEmail, string userName, string fileName, string errorMessage);
    Task SendWelcomeNotificationAsync(string userEmail, string userName);
    Task SendOrganizationInvitationNotificationAsync(string userEmail, string userName, string organizationName, string inviterName);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly HangfireEmailService _emailService;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly CustomMapOSMDbContext _context;

    public EmailNotificationService(
        HangfireEmailService emailService,
        ILogger<EmailNotificationService> logger,
        IUserRepository userRepository,
        CustomMapOSMDbContext context)
    {
        _emailService = emailService;
        _logger = logger;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task SendTransactionCompletedNotificationAsync(string userEmail, string userName, decimal amount, string planName)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "transaction_completed",
                $"Payment of ${amount:F2} for {planName} plan completed successfully", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Payment Successful - IMOS",
                Body = EmailTemplates.Notification.GetTransactionCompletedTemplate(userName, amount, planName)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Transaction completed notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue transaction completed notification for {Email}", userEmail);
        }
    }

    public async Task SendMembershipExpirationWarningAsync(string userEmail, string userName, int daysRemaining, string planName)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "membership_expiration_warning",
                $"Your {planName} membership expires in {daysRemaining} days", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = $"Membership Expires in {daysRemaining} Days - IMOS",
                Body = EmailTemplates.Notification.GetMembershipExpirationWarningTemplate(userName, daysRemaining, planName)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Membership expiration warning queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue membership expiration warning for {Email}", userEmail);
        }
    }

    public async Task SendMembershipExpiredNotificationAsync(string userEmail, string userName, string planName)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "membership_expired",
                $"Your {planName} membership has expired", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Membership Expired - IMOS",
                Body = EmailTemplates.Notification.GetMembershipExpiredTemplate(userName, planName)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Membership expired notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue membership expired notification for {Email}", userEmail);
        }
    }

    public async Task SendQuotaExceededNotificationAsync(string userEmail, string userName, string quotaType, int currentUsage, int limit)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "quota_exceeded",
                $"You have exceeded your {quotaType} quota ({currentUsage}/{limit})", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = $"Quota Exceeded - {quotaType} - IMOS",
                Body = EmailTemplates.Notification.GetQuotaExceededTemplate(userName, quotaType, currentUsage, limit)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Quota exceeded notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue quota exceeded notification for {Email}", userEmail);
        }
    }

    public async Task SendQuotaWarningNotificationAsync(string userEmail, string userName, string quotaType, int currentUsage, int limit, int percentageUsed)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "quota_warning",
                $"You have used {percentageUsed}% of your {quotaType} quota ({currentUsage}/{limit})", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = $"Quota Warning - {quotaType} - IMOS",
                Body = EmailTemplates.Notification.GetQuotaWarningTemplate(userName, quotaType, currentUsage, limit, percentageUsed)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Quota warning notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue quota warning notification for {Email}", userEmail);
        }
    }

    public async Task SendExportCompletedNotificationAsync(string userEmail, string userName, string fileName, string fileSize, string downloadLink)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "export_completed",
                $"Your export '{fileName}' ({fileSize}) is ready for download", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Export Completed - IMOS",
                Body = EmailTemplates.Notification.GetExportCompletedTemplate(userName, fileName, fileSize, downloadLink)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Export completed notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue export completed notification for {Email}", userEmail);
        }
    }

    public async Task SendExportFailedNotificationAsync(string userEmail, string userName, string fileName, string errorMessage)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "export_failed",
                $"Your export '{fileName}' failed: {errorMessage}", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Export Failed - IMOS",
                Body = EmailTemplates.Notification.GetExportFailedTemplate(userName, fileName, errorMessage)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Export failed notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue export failed notification for {Email}", userEmail);
        }
    }

    public async Task SendWelcomeNotificationAsync(string userEmail, string userName)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "welcome",
                "Welcome to IMOS! Your account has been created successfully.", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Welcome to IMOS!",
                Body = EmailTemplates.Notification.GetWelcomeTemplate(userName)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Welcome notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue welcome notification for {Email}", userEmail);
        }
    }

    public async Task SendOrganizationInvitationNotificationAsync(string userEmail, string userName, string organizationName, string inviterName)
    {
        try
        {
            // Get user ID for notification record
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            var userId = user?.UserId ?? Guid.Empty;

            // Create notification record in database
            await CreateNotificationRecordAsync(userId, "organization_invitation",
                $"{inviterName} has invited you to join {organizationName}", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = $"Invitation to join {organizationName} - IMOS",
                Body = EmailTemplates.Notification.GetOrganizationInvitationTemplate(userName, organizationName, inviterName)
            };

            _emailService.EnqueueEmail(mailRequest);
            _logger.LogInformation("Organization invitation notification queued for {Email}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue organization invitation notification for {Email}", userEmail);
        }
    }

    #region Helper Methods

    private async Task CreateNotificationRecordAsync(Guid userId, string type, string message, string status)
    {
        try
        {
            if (userId == Guid.Empty) return; // Skip if no valid user ID

            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                SentAt = status == "sent" ? DateTime.UtcNow : null
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification record for user {UserId}", userId);
        }
    }

    #endregion
}
