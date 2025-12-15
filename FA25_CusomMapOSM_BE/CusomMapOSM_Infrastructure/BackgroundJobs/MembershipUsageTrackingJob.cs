using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to track and update membership usage quotas
/// Implements FR-21: Track membership usage quotas (BR-21)
/// Runs periodically to update usage statistics and enforce quota limits
/// </summary>
public class MembershipUsageTrackingJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MembershipUsageTrackingJob> _logger;

    public MembershipUsageTrackingJob(
        IServiceProvider serviceProvider,
        ILogger<MembershipUsageTrackingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task UpdateUsageStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Starting membership usage statistics update");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var activeMemberships = await dbContext.Memberships
                .Include(m => m.Plan)
                .Include(m => m.Organization)
                .Where(m => m.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active && m.BillingCycleEndDate > DateTime.UtcNow)
                .ToListAsync();

            var updatedCount = 0;
            foreach (var membership in activeMemberships)
            {
                await UpdateMembershipUsageAsync(membership, dbContext);
                updatedCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Membership usage statistics update completed. Updated {Count} memberships",
                updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating usage statistics");
            throw;
        }
    }

    private async Task UpdateMembershipUsageAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Get current usage statistics for the organization
            var usageStats = await CalculateCurrentUsageAsync(membership.OrgId, dbContext);

            // Parse current stored usage
            var currentUsage = ParseCurrentUsage(membership.CurrentUsage);

            // Update with new statistics
            var updatedUsage = new
            {
                MapsCreated = usageStats.MapsCreated,
                ExportsUsed = usageStats.ExportsUsed,
                CustomLayersUploaded = usageStats.CustomLayersUploaded,
                UsersAdded = usageStats.UsersAdded,
                LastUpdated = DateTime.UtcNow,
                LastResetDate = currentUsage.LastResetDate ?? membership.LastResetDate
            };

            membership.CurrentUsage = JsonConvert.SerializeObject(updatedUsage);
            membership.UpdatedAt = DateTime.UtcNow;

            // Check if quotas are exceeded and log warnings
            await CheckQuotaLimitsAsync(membership, usageStats);

            _logger.LogDebug(
                "Updated usage for membership {MembershipId}: Maps={Maps}, Exports={Exports}, Layers={Layers}, Users={Users}",
                membership.MembershipId, usageStats.MapsCreated, usageStats.ExportsUsed,
                usageStats.CustomLayersUploaded, usageStats.UsersAdded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update usage for membership {MembershipId}",
                membership.MembershipId);
            throw;
        }
    }

    private async Task<UsageStatistics> CalculateCurrentUsageAsync(Guid orgId, CustomMapOSMDbContext dbContext)
    {
        var startOfMonth = GetStartOfCurrentBillingCycle(orgId, dbContext);

        var mapsCreated = await dbContext.Maps
            .Where(m => m.Workspace != null
                        && m.Workspace.Organization != null
                        && m.Workspace.Organization.OrgId == orgId
                        && m.CreatedAt >= startOfMonth)
            .CountAsync();

        var exportsUsed = await dbContext.Exports
            .Where(e => e.Membership.OrgId == orgId && e.CreatedAt >= startOfMonth)
            .CountAsync();

        // var customLayersUploaded = await dbContext.Layers
        //     .Where(cl => cl.UserId == orgId && cl.CreatedAt >= startOfMonth)
        //     .CountAsync();

        var usersAdded = await dbContext.OrganizationMembers
            .Where(om => om.OrgId == orgId && om.JoinedAt >= startOfMonth)
            .CountAsync();

        return new UsageStatistics
        {
            MapsCreated = mapsCreated,
            ExportsUsed = exportsUsed,
            // CustomLayersUploaded = customLayersUploaded,
            UsersAdded = usersAdded
        };
    }

    private DateTime GetStartOfCurrentBillingCycle(Guid orgId, CustomMapOSMDbContext dbContext)
    {
        // Get the membership start date to determine billing cycle
        var membership = dbContext.Memberships
            .FirstOrDefault(m => m.OrgId == orgId && m.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active);

        if (membership?.LastResetDate != null)
        {
            return membership.LastResetDate.Value;
        }

        // Fallback to start of current month if no reset date
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, 1);
    }

    private dynamic ParseCurrentUsage(string? currentUsageJson)
    {
        if (string.IsNullOrEmpty(currentUsageJson))
        {
            return new
            {
                MapsCreated = 0,
                ExportsUsed = 0,
                CustomLayersUploaded = 0,
                UsersAdded = 0,
                LastResetDate = (DateTime?)null
            };
        }

        try
        {
            return JsonConvert.DeserializeObject(currentUsageJson) ?? new
            {
                MapsCreated = 0,
                ExportsUsed = 0,
                CustomLayersUploaded = 0,
                UsersAdded = 0,
                LastResetDate = (DateTime?)null
            };
        }
        catch
        {
            return new
            {
                MapsCreated = 0,
                ExportsUsed = 0,
                CustomLayersUploaded = 0,
                UsersAdded = 0,
                LastResetDate = (DateTime?)null
            };
        }
    }

    private async Task CheckQuotaLimitsAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        UsageStatistics usageStats)
    {
        var plan = membership.Plan!;
        var quotaExceeded = new List<string>();

        if (usageStats.MapsCreated > plan.MapQuota)
            quotaExceeded.Add($"Maps ({usageStats.MapsCreated}/{plan.MapQuota})");

        if (usageStats.ExportsUsed > plan.ExportQuota)
            quotaExceeded.Add($"Exports ({usageStats.ExportsUsed}/{plan.ExportQuota})");

        if (usageStats.CustomLayersUploaded > plan.MaxCustomLayers)
            quotaExceeded.Add($"Custom Layers ({usageStats.CustomLayersUploaded}/{plan.MaxCustomLayers})");

        if (usageStats.UsersAdded > plan.MaxUsersPerOrg)
            quotaExceeded.Add($"Users ({usageStats.UsersAdded}/{plan.MaxUsersPerOrg})");

        if (quotaExceeded.Any())
        {
            _logger.LogWarning(
                "Quota exceeded for membership {MembershipId}, organization {OrgId}: {ExceededQuotas}",
                membership.MembershipId, membership.OrgId, string.Join(", ", quotaExceeded));

            // Log quota exceeded event for monitoring
            await LogQuotaExceededEventAsync(membership, quotaExceeded);
        }
    }

    private async Task LogQuotaExceededEventAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        List<string> exceededQuotas)
    {
        try
        {
            var logEntry = new
            {
                EventType = "QuotaExceeded",
                MembershipId = membership.MembershipId,
                UserId = membership.UserId,
                OrgId = membership.OrgId,
                PlanId = membership.PlanId,
                ExceededQuotas = exceededQuotas,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogWarning(
                "Quota exceeded event: {LogEntry}",
                JsonConvert.SerializeObject(logEntry));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log quota exceeded event for membership {MembershipId}", membership.MembershipId);
        }
    }

    /// <summary>
    /// Manual usage update for a specific membership (can be called after specific actions)
    /// </summary>
    [Queue("default")]
    public async Task UpdateSpecificMembershipUsageAsync(Guid membershipId)
    {
        try
        {
            _logger.LogInformation("Starting manual usage update for membership {MembershipId}", membershipId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var membership = await dbContext.Memberships
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found for usage update", membershipId);
                return;
            }

            if (membership.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active)
            {
                _logger.LogWarning("Cannot update usage for inactive membership {MembershipId}", membershipId);
                return;
            }

            await UpdateMembershipUsageAsync(membership, dbContext);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Manual usage update completed for membership {MembershipId}", membershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during manual usage update for membership {MembershipId}", membershipId);
            throw;
        }
    }

    private class UsageStatistics
    {
        public int MapsCreated { get; set; }
        public int ExportsUsed { get; set; }
        public int CustomLayersUploaded { get; set; }
        public int UsersAdded { get; set; }
    }
}
