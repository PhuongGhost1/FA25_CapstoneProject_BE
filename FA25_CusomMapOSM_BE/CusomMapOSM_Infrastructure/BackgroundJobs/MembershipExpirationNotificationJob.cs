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
/// Background job to check for memberships expiring soon and send notification emails
/// Implements BR-24: Users receive notifications before membership expires (FR-24)
/// Runs daily to check memberships expiring in 7, 3, and 1 days
/// </summary>
public class MembershipExpirationNotificationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MembershipExpirationNotificationJob> _logger;

    public MembershipExpirationNotificationJob(
        IServiceProvider serviceProvider,
        ILogger<MembershipExpirationNotificationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("email")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CheckAndNotifyExpiringMembershipsAsync()
    {
        try
        {
            _logger.LogInformation("Starting membership expiration notification check");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var today = DateTime.UtcNow.Date;
            var expirationDates = new[]
            {
                today.AddDays(7), // 7 days before expiration
                today.AddDays(3), // 3 days before expiration
                today.AddDays(1)  // 1 day before expiration
            };

            foreach (var expirationDate in expirationDates)
            {
                var expiringMemberships = await dbContext.Memberships
                    .Include(m => m.User)
                    .Include(m => m.Organization)
                    .Include(m => m.Plan)
                    .Include(m => m.Status)
                    .Where(m => m.EndDate.HasValue &&
                               m.EndDate.Value.Date == expirationDate &&
                               m.Status!.Name == "Active" &&
                               m.User != null)
                    .ToListAsync();

                foreach (var membership in expiringMemberships)
                {
                    var daysUntilExpiration = (membership.EndDate!.Value.Date - today).Days;
                    await SendExpirationNotificationAsync(membership, daysUntilExpiration, hangfireEmailService);
                }
            }

            _logger.LogInformation("Membership expiration notification check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking expiring memberships");
            throw;
        }
    }

    private async Task SendExpirationNotificationAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        int daysUntilExpiration,
        HangfireEmailService hangfireEmailService)
    {
        try
        {
            var subject = GetExpirationSubject(daysUntilExpiration);
            var body = GetExpirationEmailBody(membership, daysUntilExpiration);

            var mailRequest = new MailRequest
            {
                ToEmail = membership.User!.Email,
                Subject = subject,
                Body = body
            };

            var jobId = hangfireEmailService.EnqueueEmail(mailRequest);
            await Task.CompletedTask; // Make it properly async

            _logger.LogInformation(
                "Expiration notification queued for user {UserId}, membership expires in {Days} days",
                membership.UserId, daysUntilExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue expiration notification for user {UserId}",
                membership.UserId);
        }
    }

    private string GetExpirationSubject(int daysUntilExpiration)
    {
        return daysUntilExpiration switch
        {
            7 => "Your Custom Map OSM membership expires in 7 days",
            3 => "Your Custom Map OSM membership expires in 3 days - Action Required",
            1 => "URGENT: Your Custom Map OSM membership expires tomorrow",
            _ => "Your Custom Map OSM membership expires soon"
        };
    }

    private string GetExpirationEmailBody(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        int daysUntilExpiration)
    {
        var urgencyClass = daysUntilExpiration switch
        {
            1 => "urgent",
            3 => "warning",
            _ => "info"
        };

        var actionText = daysUntilExpiration switch
        {
            1 => "Please renew immediately to avoid service interruption.",
            3 => "Please consider renewing your membership to continue enjoying our services.",
            _ => "Please plan to renew your membership to avoid any service interruption."
        };

        return $@"
            <div class=""notification {urgencyClass}"">
                <h2>Membership Expiration Notice</h2>
                <p>Dear {membership.User!.FullName ?? membership.User.Email},</p>
                
                <p>This is a friendly reminder that your <strong>{membership.Plan!.PlanName}</strong> membership 
                for organization <strong>{membership.Organization!.OrgName}</strong> will expire in 
                <strong>{daysUntilExpiration} day{(daysUntilExpiration == 1 ? "" : "s")}</strong> 
                on {membership.EndDate!.Value:MMMM dd, yyyy}.</p>
                
                <p>{actionText}</p>
                
                <div class=""membership-details"">
                    <h3>Current Membership Details:</h3>
                    <ul>
                        <li><strong>Plan:</strong> {membership.Plan.PlanName}</li>
                        <li><strong>Organization:</strong> {membership.Organization.OrgName}</li>
                        <li><strong>Expiration Date:</strong> {membership.EndDate.Value:MMMM dd, yyyy}</li>
                        <li><strong>Auto-renewal:</strong> {(membership.AutoRenew ? "Enabled" : "Disabled")}</li>
                    </ul>
                </div>
                
                <div class=""action-buttons"">
                    <a href=""https://yourdomain.com/membership/renew"" class=""btn btn-primary"">
                        Renew Membership
                    </a>
                    <a href=""https://yourdomain.com/membership/upgrade"" class=""btn btn-secondary"">
                        Upgrade Plan
                    </a>
                </div>
                
                <p>If you have any questions or need assistance, please contact our support team.</p>
                
                <p>Thank you for using Custom Map OSM!</p>
            </div>";
    }
}
