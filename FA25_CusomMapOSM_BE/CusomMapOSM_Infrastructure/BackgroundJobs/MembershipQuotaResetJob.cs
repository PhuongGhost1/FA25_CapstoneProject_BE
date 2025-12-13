using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to reset monthly quotas on membership anniversary dates
/// Implements BR-13: Membership quotas reset monthly on the anniversary date (FR-21)
/// Runs daily to check for memberships that need quota reset
/// </summary>
public class MembershipQuotaResetJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MembershipQuotaResetJob> _logger;

    public MembershipQuotaResetJob(
        IServiceProvider serviceProvider,
        ILogger<MembershipQuotaResetJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task ResetMonthlyQuotasAsync()
    {
        try
        {
            _logger.LogInformation("Starting monthly quota reset process");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var today = DateTime.UtcNow.Date;
            var membershipsToReset = await dbContext.Memberships
                .Include(m => m.Plan)
                .Include(m => m.Status)
                .Where(m => m.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active &&
                           m.BillingCycleEndDate > today && // Only active memberships
                           (m.LastResetDate == null ||
                            ShouldResetQuota(m.LastResetDate.Value, m.BillingCycleStartDate, today)))
                .ToListAsync();

            var resetCount = 0;
            foreach (var membership in membershipsToReset)
            {
                await ResetMembershipQuotasAsync(membership, dbContext);
                resetCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly quota reset completed. Reset quotas for {Count} memberships",
                resetCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting monthly quotas");
            throw;
        }
    }

    private bool ShouldResetQuota(DateTime lastResetDate, DateTime membershipStartDate, DateTime today)
    {
        // Calculate the anniversary day of the month based on membership start date
        var anniversaryDay = membershipStartDate.Day;

        // If today is the anniversary day and it's been at least 30 days since last reset
        if (today.Day == anniversaryDay && (today - lastResetDate).TotalDays >= 30)
        {
            return true;
        }

        // Handle month-end cases (e.g., membership started on 31st but current month has 30 days)
        var daysInCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);
        if (anniversaryDay > daysInCurrentMonth && today.Day == daysInCurrentMonth)
        {
            return (today - lastResetDate).TotalDays >= 30;
        }

        return false;
    }

    private async Task ResetMembershipQuotasAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Reset current usage to default values based on plan
            var defaultUsage = new
            {
                MapsCreated = 0,
                ExportsUsed = 0,
                CustomLayersUploaded = 0,
                UsersAdded = 0,
                LastResetDate = DateTime.UtcNow
            };

            membership.CurrentUsage = JsonConvert.SerializeObject(defaultUsage);
            membership.LastResetDate = DateTime.UtcNow;
            membership.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Reset quotas for membership {MembershipId}, user {UserId}, organization {OrgId}",
                membership.MembershipId, membership.UserId, membership.OrgId);

            // Log the quota reset event for audit purposes
            await LogQuotaResetEventAsync(membership, dbContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to reset quotas for membership {MembershipId}",
                membership.MembershipId);
            throw;
        }
    }

    private async Task LogQuotaResetEventAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Create a system log entry for quota reset
            var logEntry = new
            {
                EventType = "QuotaReset",
                MembershipId = membership.MembershipId,
                UserId = membership.UserId,
                OrgId = membership.OrgId,
                PlanId = membership.PlanId,
                ResetDate = DateTime.UtcNow,
                PreviousUsage = membership.CurrentUsage,
                NewUsage = JsonConvert.SerializeObject(new
                {
                    MapsCreated = 0,
                    ExportsUsed = 0,
                    CustomLayersUploaded = 0,
                    UsersAdded = 0,
                    LastResetDate = DateTime.UtcNow
                })
            };

            // Store in system logs table if it exists, otherwise just log
            _logger.LogInformation(
                "Quota reset event logged: {LogEntry}",
                JsonConvert.SerializeObject(logEntry));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log quota reset event for membership {MembershipId}", membership.MembershipId);
        }
    }

    /// <summary>
    /// Manual quota reset for a specific membership (can be called from admin interface)
    /// </summary>
    [Queue("default")]
    public async Task ResetSpecificMembershipQuotasAsync(Guid membershipId)
    {
        try
        {
            _logger.LogInformation("Starting manual quota reset for membership {MembershipId}", membershipId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var membership = await dbContext.Memberships
                .Include(m => m.Plan)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found for quota reset", membershipId);
                return;
            }

            if (membership.Status != CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active)
            {
                _logger.LogWarning("Cannot reset quotas for inactive membership {MembershipId}", membershipId);
                return;
            }

            await ResetMembershipQuotasAsync(membership, dbContext);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Manual quota reset completed for membership {MembershipId}", membershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during manual quota reset for membership {MembershipId}", membershipId);
            throw;
        }
    }
}
