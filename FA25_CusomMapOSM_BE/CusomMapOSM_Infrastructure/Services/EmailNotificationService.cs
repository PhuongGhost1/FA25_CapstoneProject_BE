using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
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
                Subject = "Payment Successful - Custom Map OSM",
                Body = CreateTransactionCompletedHtml(userName, amount, planName)
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
                Subject = $"Membership Expires in {daysRemaining} Days - Custom Map OSM",
                Body = CreateMembershipExpirationWarningHtml(userName, daysRemaining, planName)
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
                Subject = "Membership Expired - Custom Map OSM",
                Body = CreateMembershipExpiredHtml(userName, planName)
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
                Subject = $"Quota Exceeded - {quotaType} - Custom Map OSM",
                Body = CreateQuotaExceededHtml(userName, quotaType, currentUsage, limit)
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
                Subject = $"Quota Warning - {quotaType} - Custom Map OSM",
                Body = CreateQuotaWarningHtml(userName, quotaType, currentUsage, limit, percentageUsed)
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
                Subject = "Export Completed - Custom Map OSM",
                Body = CreateExportCompletedHtml(userName, fileName, fileSize, downloadLink)
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
                Subject = "Export Failed - Custom Map OSM",
                Body = CreateExportFailedHtml(userName, fileName, errorMessage)
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
                "Welcome to Custom Map OSM! Your account has been created successfully.", "sent");

            // Send email
            var mailRequest = new MailRequest
            {
                ToEmail = userEmail,
                Subject = "Welcome to Custom Map OSM!",
                Body = CreateWelcomeHtml(userName)
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
                Subject = $"Invitation to join {organizationName} - Custom Map OSM",
                Body = CreateOrganizationInvitationHtml(userName, organizationName, inviterName)
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

    #region HTML Templates

    private string CreateTransactionCompletedHtml(string userName, decimal amount, string planName)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Payment Successful!</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.9;"">Your transaction has been completed</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">Great news! Your payment has been processed successfully.</p>
                
                <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                    <h3 style=""color: #2d3748; margin-top: 0;"">Transaction Details</h3>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>Plan:</strong> {planName}</p>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>Amount:</strong> ${amount:F2}</p>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</p>
                </div>
                
                <p style=""color: #4a5568; line-height: 1.6;"">Your membership is now active and you can access all premium features.</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/dashboard"" style=""background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Go to Dashboard</a>
                </div>
            </div>
        </div>";
    }

    private string CreateMembershipExpirationWarningHtml(string userName, int daysRemaining, string planName)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Membership Expiring Soon</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.9;"">Only {daysRemaining} days remaining</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">Your {planName} membership will expire in {daysRemaining} days.</p>
                
                <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0;"">
                    <h3 style=""color: #c53030; margin-top: 0;"">⚠️ Action Required</h3>
                    <p style=""color: #4a5568; margin: 0;"">To continue enjoying premium features, please renew your membership before it expires.</p>
                </div>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/membership/renew"" style=""background: #f56565; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Renew Membership</a>
                </div>
            </div>
        </div>";
    }

    private string CreateMembershipExpiredHtml(string userName, string planName)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%); color: #8b4513; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Membership Expired</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.8;"">Your {planName} membership has expired</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">Your {planName} membership has expired. You now have limited access to features.</p>
                
                <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0;"">
                    <h3 style=""color: #c53030; margin-top: 0;"">Limited Access</h3>
                    <p style=""color: #4a5568; margin: 0;"">Some features may be restricted until you renew your membership.</p>
                </div>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/membership/renew"" style=""background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Renew Membership</a>
                </div>
            </div>
        </div>";
    }

    private string CreateQuotaExceededHtml(string userName, string quotaType, int currentUsage, int limit)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%); color: #8b4513; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Quota Exceeded</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.8;"">You've reached your {quotaType} limit</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">You have exceeded your {quotaType} quota for this billing period.</p>
                
                <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0;"">
                    <h3 style=""color: #c53030; margin-top: 0;"">Usage Details</h3>
                    <p style=""color: #4a5568; margin: 5px 0;""><strong>Current Usage:</strong> {currentUsage}</p>
                    <p style=""color: #4a5568; margin: 5px 0;""><strong>Limit:</strong> {limit}</p>
                </div>
                
                <p style=""color: #4a5568; line-height: 1.6;"">Consider upgrading your plan to increase your quota limits.</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/membership/upgrade"" style=""background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Upgrade Plan</a>
                </div>
            </div>
        </div>";
    }

    private string CreateQuotaWarningHtml(string userName, string quotaType, int currentUsage, int limit, int percentageUsed)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%); color: #8b4513; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Quota Warning</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.8;"">You're at {percentageUsed}% of your {quotaType} limit</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">You've used {percentageUsed}% of your {quotaType} quota for this billing period.</p>
                
                <div style=""background: #fffaf0; border-left: 4px solid #ed8936; padding: 20px; margin: 20px 0;"">
                    <h3 style=""color: #c05621; margin-top: 0;"">Usage Progress</h3>
                    <div style=""background: #e2e8f0; border-radius: 10px; height: 20px; margin: 10px 0;"">
                        <div style=""background: #ed8936; height: 100%; border-radius: 10px; width: {percentageUsed}%;""></div>
                    </div>
                    <p style=""color: #4a5568; margin: 5px 0;""><strong>Used:</strong> {currentUsage} / {limit}</p>
                </div>
                
                <p style=""color: #4a5568; line-height: 1.6;"">Consider monitoring your usage or upgrading your plan if you need more capacity.</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/dashboard/usage"" style=""background: #ed8936; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">View Usage</a>
                </div>
            </div>
        </div>";
    }

    private string CreateExportCompletedHtml(string userName, string fileName, string fileSize, string downloadLink)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Export Ready!</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.9;"">Your map export is complete</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">Your map export has been successfully generated and is ready for download.</p>
                
                <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                    <h3 style=""color: #2d3748; margin-top: 0;"">Export Details</h3>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>File:</strong> {fileName}</p>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>Size:</strong> {fileSize}</p>
                    <p style=""margin: 5px 0; color: #4a5568;""><strong>Generated:</strong> {DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC</p>
                </div>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{downloadLink}"" style=""background: #48bb78; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Download File</a>
                </div>
                
                <p style=""color: #718096; font-size: 14px; text-align: center;"">This download link will expire in 30 days.</p>
            </div>
        </div>";
    }

    private string CreateExportFailedHtml(string userName, string fileName, string errorMessage)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%); color: #8b4513; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Export Failed</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.8;"">There was an issue with your export</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">We encountered an issue while generating your export.</p>
                
                <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0;"">
                    <h3 style=""color: #c53030; margin-top: 0;"">Error Details</h3>
                    <p style=""color: #4a5568; margin: 5px 0;""><strong>File:</strong> {fileName}</p>
                    <p style=""color: #4a5568; margin: 5px 0;""><strong>Error:</strong> {errorMessage}</p>
                </div>
                
                <p style=""color: #4a5568; line-height: 1.6;"">Please try again or contact support if the issue persists.</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/support"" style=""background: #f56565; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Contact Support</a>
                </div>
            </div>
        </div>";
    }

    private string CreateWelcomeHtml(string userName)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">Welcome to Custom Map OSM!</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.9;"">Your mapping journey starts here</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">Welcome to Custom Map OSM! We're excited to have you join our community of map creators and GIS professionals.</p>
                
                <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                    <h3 style=""color: #2d3748; margin-top: 0;"">Getting Started</h3>
                    <ul style=""color: #4a5568; margin: 0; padding-left: 20px;"">
                        <li>Create your first custom map</li>
                        <li>Upload your own data layers</li>
                        <li>Collaborate with team members</li>
                        <li>Export maps in various formats</li>
                    </ul>
                </div>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/dashboard"" style=""background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Get Started</a>
                </div>
            </div>
        </div>";
    }

    private string CreateOrganizationInvitationHtml(string userName, string organizationName, string inviterName)
    {
        return $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
            <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                <h1 style=""margin: 0; font-size: 28px;"">You're Invited!</h1>
                <p style=""margin: 10px 0 0 0; opacity: 0.9;"">Join {organizationName} on Custom Map OSM</p>
            </div>
            <div style=""background: white; padding: 30px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 10px 10px;"">
                <h2 style=""color: #2d3748; margin-top: 0;"">Hello {userName},</h2>
                <p style=""color: #4a5568; line-height: 1.6;"">{inviterName} has invited you to join <strong>{organizationName}</strong> on Custom Map OSM.</p>
                
                <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                    <h3 style=""color: #2d3748; margin-top: 0;"">What you'll get:</h3>
                    <ul style=""color: #4a5568; margin: 0; padding-left: 20px;"">
                        <li>Access to shared maps and data</li>
                        <li>Collaborative mapping tools</li>
                        <li>Team management features</li>
                        <li>Advanced export options</li>
                    </ul>
                </div>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""https://yourdomain.com/organization/accept"" style=""background: #48bb78; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Accept Invitation</a>
                </div>
            </div>
        </div>";
    }

    #endregion
}
