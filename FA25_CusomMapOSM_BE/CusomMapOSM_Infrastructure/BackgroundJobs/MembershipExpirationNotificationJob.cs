using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Domain.Entities.Memberships.Enums;

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
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var today = DateTime.UtcNow.Date;
            var expirationDates = new[]
            {
                today.AddDays(7), // 7 days before expiration
                today.AddDays(3), // 3 days before expiration
                today.AddDays(1)  // 1 day before expiration
            };

            const int PREMIUM_PLAN_ID = 2; // Premium plan ID
            const int FREE_PLAN_ID = 1; // Free plan ID

            foreach (var expirationDate in expirationDates)
            {
                var expiringMemberships = await dbContext.Memberships
                    .Include(m => m.User)
                    .Include(m => m.Organization)
                    .Include(m => m.Plan)
                    .Include(m => m.Status)
                    .Where(m => m.EndDate.HasValue &&
                               m.EndDate.Value.Date == expirationDate &&
                               m.Status! == MembershipStatusEnum.Active &&
                               m.User != null)
                    .ToListAsync();

                foreach (var membership in expiringMemberships)
                {
                    var daysUntilExpiration = (membership.EndDate!.Value.Date - today).Days;

                    // Send expiration notification
                    await SendExpirationNotificationAsync(membership, daysUntilExpiration, hangfireEmailService, notificationService);

                    // Auto-downgrade Premium memberships (Plan 2) expiring in 7 days
                    // If user hasn't extended by now, downgrade to Free (Plan 1)
                    if (daysUntilExpiration == 7 && membership.PlanId == PREMIUM_PLAN_ID)
                    {
                        await AutoDowngradeToFreeAsync(membership, scope);
                    }
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
        HangfireEmailService hangfireEmailService,
        INotificationService notificationService)
    {
        try
        {
            // Use the new NotificationService for both database record and email
            await notificationService.SendMembershipExpirationWarningAsync(
                membership.User!.Email,
                membership.User.FullName ?? "User",
                daysUntilExpiration,
                membership.Plan?.PlanName ?? "Unknown Plan");

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

    private async Task AutoDowngradeToFreeAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        IServiceScope scope)
    {
        try
        {
            const int FREE_PLAN_ID = 1;

            _logger.LogInformation(
                "Auto-downgrading Premium membership {MembershipId} for user {UserId} in organization {OrgId} to Free plan",
                membership.MembershipId, membership.UserId, membership.OrgId);

            var membershipService = scope.ServiceProvider.GetRequiredService<IMembershipService>();

            // Downgrade to Free plan (Plan 1)
            // Note: Since we're within 7 days, this should be allowed by the downgrade logic
            var downgradeResult = await membershipService.CreateOrRenewMembershipAsync(
                membership.UserId,
                membership.OrgId,
                FREE_PLAN_ID,
                false, // Auto-renew disabled for free plan
                CancellationToken.None);

            if (downgradeResult.HasValue)
            {
                _logger.LogInformation(
                    "Successfully auto-downgraded membership {MembershipId} to Free plan",
                    membership.MembershipId);
            }
            else
            {
                var error = downgradeResult.Match(
                    some: _ => (Error?)null,
                    none: err => err);
                _logger.LogWarning(
                    "Failed to auto-downgrade membership {MembershipId}: {Error}",
                    membership.MembershipId, error?.Description ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while auto-downgrading membership {MembershipId}",
                membership.MembershipId);
        }
    }

}
