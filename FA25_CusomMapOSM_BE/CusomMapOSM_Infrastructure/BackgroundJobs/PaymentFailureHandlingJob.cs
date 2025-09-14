using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to handle payment failures and suspend memberships
/// Implements BR-19: Failed payments result in membership suspension after 3 days (FR-25, FR-24)
/// Runs daily to check for failed payments and suspend memberships accordingly
/// </summary>
public class PaymentFailureHandlingJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentFailureHandlingJob> _logger;

    public PaymentFailureHandlingJob(
        IServiceProvider serviceProvider,
        ILogger<PaymentFailureHandlingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task HandleFailedPaymentsAsync()
    {
        try
        {
            _logger.LogInformation("Starting payment failure handling process");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

            // Find memberships with failed payments that need suspension
            var membershipsToSuspend = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .Include(m => m.Status)
                .Where(m => m.Status!.Name == "PendingPayment" &&
                           m.UpdatedAt <= threeDaysAgo) // Failed payment for 3+ days
                .ToListAsync();

            var suspendedCount = 0;
            foreach (var membership in membershipsToSuspend)
            {
                await SuspendMembershipForFailedPaymentAsync(membership, dbContext, hangfireEmailService);
                suspendedCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Payment failure handling completed. Suspended {Count} memberships",
                suspendedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling failed payments");
            throw;
        }
    }

    private async Task SuspendMembershipForFailedPaymentAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CustomMapOSMDbContext dbContext,
        HangfireEmailService hangfireEmailService)
    {
        try
        {
            // Update membership status to Suspended
            var suspendedStatus = await dbContext.MembershipStatuses
                .FirstOrDefaultAsync(ms => ms.Name == "Suspended");

            if (suspendedStatus != null)
            {
                membership.StatusId = suspendedStatus.StatusId;
                membership.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Suspended membership {MembershipId} for user {UserId} due to failed payment",
                    membership.MembershipId, membership.UserId);

                // Send suspension notification email
                await SendSuspensionNotificationAsync(membership, hangfireEmailService);

                // Log the suspension event
                await LogMembershipSuspensionEventAsync(membership);
            }
            else
            {
                _logger.LogError("Suspended status not found in database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to suspend membership {MembershipId} for failed payment",
                membership.MembershipId);
            throw;
        }
    }

    private async Task SendSuspensionNotificationAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        HangfireEmailService hangfireEmailService)
    {
        try
        {
            var subject = "Membership Suspended - Payment Required";
            var body = GetSuspensionEmailBody(membership);

            var mailRequest = new MailRequest
            {
                ToEmail = membership.User!.Email,
                Subject = subject,
                Body = body
            };

            hangfireEmailService.EnqueueEmail(mailRequest);

            _logger.LogInformation(
                "Suspension notification queued for user {UserId}",
                membership.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue suspension notification for user {UserId}",
                membership.UserId);
        }
    }

    private string GetSuspensionEmailBody(CusomMapOSM_Domain.Entities.Memberships.Membership membership)
    {
        return $@"
            <div class=""notification urgent"">
                <h2>Membership Suspended - Payment Required</h2>
                <p>Dear {membership.User!.FullName ?? membership.User.Email},</p>
                
                <p>We regret to inform you that your <strong>{membership.Plan!.PlanName}</strong> membership 
                for organization <strong>{membership.Organization!.OrgName}</strong> has been 
                <strong>suspended</strong> due to a failed payment.</p>
                
                <div class=""alert alert-warning"">
                    <h3>What this means:</h3>
                    <ul>
                        <li>Your access to premium features has been temporarily disabled</li>
                        <li>Your organization's maps and data remain safe and secure</li>
                        <li>You can restore access by updating your payment information</li>
                    </ul>
                </div>
                
                <div class=""membership-details"">
                    <h3>Membership Details:</h3>
                    <ul>
                        <li><strong>Plan:</strong> {membership.Plan.PlanName}</li>
                        <li><strong>Organization:</strong> {membership.Organization.OrgName}</li>
                        <li><strong>Suspension Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</li>
                        <li><strong>Reason:</strong> Failed payment after 3 days</li>
                    </ul>
                </div>
                
                <div class=""action-buttons"">
                    <a href=""https://yourdomain.com/membership/payment"" class=""btn btn-primary"">
                        Update Payment Information
                    </a>
                    <a href=""https://yourdomain.com/support"" class=""btn btn-secondary"">
                        Contact Support
                    </a>
                </div>
                
                <p>If you believe this is an error or need assistance, please contact our support team immediately.</p>
                
                <p>We're here to help restore your access as quickly as possible.</p>
            </div>";
    }

    private async Task LogMembershipSuspensionEventAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership)
    {
        try
        {
            var logEntry = new
            {
                EventType = "MembershipSuspension",
                MembershipId = membership.MembershipId,
                UserId = membership.UserId,
                OrgId = membership.OrgId,
                PlanId = membership.PlanId,
                SuspensionDate = DateTime.UtcNow,
                Reason = "Failed payment after 3 days",
                PreviousStatus = "PendingPayment"
            };

            _logger.LogWarning(
                "Membership suspension event: {LogEntry}",
                System.Text.Json.JsonSerializer.Serialize(logEntry));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log membership suspension event for membership {MembershipId}", membership.MembershipId);
        }
    }

    /// <summary>
    /// Check for memberships that need payment failure warnings (1 day before suspension)
    /// </summary>
    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task SendPaymentFailureWarningsAsync()
    {
        try
        {
            _logger.LogInformation("Starting payment failure warning process");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

            // Find memberships with failed payments that need warning (2 days old)
            var membershipsNeedingWarning = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .Include(m => m.Status)
                .Where(m => m.Status!.Name == "PendingPayment" &&
                           m.UpdatedAt <= twoDaysAgo &&
                           m.UpdatedAt > DateTime.UtcNow.AddDays(-3)) // Between 2-3 days old
                .ToListAsync();

            var warningCount = 0;
            foreach (var membership in membershipsNeedingWarning)
            {
                await SendPaymentFailureWarningAsync(membership, hangfireEmailService);
                warningCount++;
            }

            _logger.LogInformation(
                "Payment failure warning process completed. Sent {Count} warnings",
                warningCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending payment failure warnings");
            throw;
        }
    }

    private async Task SendPaymentFailureWarningAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        HangfireEmailService hangfireEmailService)
    {
        try
        {
            var subject = "Payment Failed - Action Required to Prevent Suspension";
            var body = GetPaymentFailureWarningBody(membership);

            var mailRequest = new MailRequest
            {
                ToEmail = membership.User!.Email,
                Subject = subject,
                Body = body
            };

            hangfireEmailService.EnqueueEmail(mailRequest);

            _logger.LogInformation(
                "Payment failure warning queued for user {UserId}",
                membership.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue payment failure warning for user {UserId}",
                membership.UserId);
        }
    }

    private string GetPaymentFailureWarningBody(CusomMapOSM_Domain.Entities.Memberships.Membership membership)
    {
        return $@"
            <div class=""notification warning"">
                <h2>Payment Failed - Action Required</h2>
                <p>Dear {membership.User!.FullName ?? membership.User.Email},</p>
                
                <p>We were unable to process the payment for your <strong>{membership.Plan!.PlanName}</strong> membership 
                for organization <strong>{membership.Organization!.OrgName}</strong>.</p>
                
                <div class=""alert alert-warning"">
                    <h3>Important Notice:</h3>
                    <p>Your membership will be <strong>suspended in 1 day</strong> if payment is not updated.</p>
                    <p>This will temporarily disable your access to premium features.</p>
                </div>
                
                <div class=""action-buttons"">
                    <a href=""https://yourdomain.com/membership/payment"" class=""btn btn-primary"">
                        Update Payment Information Now
                    </a>
                    <a href=""https://yourdomain.com/support"" class=""btn btn-secondary"">
                        Contact Support
                    </a>
                </div>
                
                <p>If you have any questions or need assistance, please contact our support team immediately.</p>
                
                <p>Thank you for your prompt attention to this matter.</p>
            </div>";
    }

    /// <summary>
    /// Manual suspension of specific membership (can be called from admin interface)
    /// </summary>
    [Queue("default")]
    public async Task SuspendSpecificMembershipAsync(Guid membershipId, string reason = "Manual suspension")
    {
        try
        {
            _logger.LogInformation("Starting manual suspension for membership {MembershipId}", membershipId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var membership = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found for suspension", membershipId);
                return;
            }

            if (membership.Status!.Name == "Suspended")
            {
                _logger.LogWarning("Membership {MembershipId} is already suspended", membershipId);
                return;
            }

            await SuspendMembershipForFailedPaymentAsync(membership, dbContext, hangfireEmailService);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Manual membership suspension completed for {MembershipId}", membershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during manual membership suspension for {MembershipId}", membershipId);
            throw;
        }
    }
}
