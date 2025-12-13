using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CusomMapOSM_Domain.Entities.Memberships.Enums;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to automatically downgrade expired memberships to Free Plan.
/// Runs daily to check memberships where BillingCycleEndDate has passed.
/// </summary>
public class MembershipAutoDowngradeJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MembershipAutoDowngradeJob> _logger;

    private const int FREE_PLAN_ID = 1; // Free plan ID

    public MembershipAutoDowngradeJob(
        IServiceProvider serviceProvider,
        ILogger<MembershipAutoDowngradeJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task AutoDowngradeExpiredMembershipsAsync()
    {
        try
        {
            _logger.LogInformation("Starting auto-downgrade check for expired memberships");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var membershipRepository = scope.ServiceProvider.GetRequiredService<IMembershipRepository>();

            var now = DateTime.UtcNow;

            // Find all active memberships that have expired (billing cycle ended)
            // AND are not already on free plan
            var expiredMemberships = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .Where(m => m.Status == MembershipStatusEnum.Active &&
                           m.BillingCycleEndDate < now &&
                           m.PlanId != FREE_PLAN_ID) // Don't downgrade if already on free plan
                .ToListAsync();

            _logger.LogInformation("Found {Count} expired memberships to downgrade", expiredMemberships.Count);

            int successCount = 0;
            int failureCount = 0;

            foreach (var membership in expiredMemberships)
            {
                try
                {
                    _logger.LogInformation(
                        "Auto-downgrading membership {MembershipId} for user {UserId} in organization {OrgId} from plan {PlanId} to Free plan",
                        membership.MembershipId, membership.UserId, membership.OrgId, membership.PlanId);

                    // Directly update membership to Free plan (bypassing validation that prevents downgrades)
                    // Since billing cycle has expired, we can safely downgrade
                    membership.PlanId = FREE_PLAN_ID;
                    membership.AutoRenew = false; // Free plan doesn't auto-renew
                    membership.Status = MembershipStatusEnum.Active;
                    membership.UpdatedAt = now;
                    
                    // Reset billing cycle dates for the new Free plan period
                    membership.BillingCycleStartDate = now;
                    membership.BillingCycleEndDate = now.AddDays(30); // 30-day billing cycle for Free plan

                    // Reset usage cycle to give fresh start with Free plan quotas
                    membership.LastResetDate = now;
                    
                    // Update membership in database
                    await membershipRepository.UpsertAsync(membership, CancellationToken.None);

                    // Reset usage statistics for the organization
                    var usage = await membershipRepository.GetUsageAsync(membership.MembershipId, membership.OrgId, CancellationToken.None);
                    if (usage != null)
                    {
                        usage.MapsCreatedThisCycle = 0;
                        usage.ExportsThisCycle = 0;
                        usage.ActiveUsersInOrg = 0;
                        usage.CycleStartDate = now;
                        usage.CycleEndDate = now.AddMonths(1);
                        usage.UpdatedAt = now;
                        await membershipRepository.UpsertUsageAsync(usage, CancellationToken.None);
                    }

                    successCount++;
                    _logger.LogInformation(
                        "Successfully auto-downgraded membership {MembershipId} to Free plan",
                        membership.MembershipId);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex,
                        "Error occurred while auto-downgrading membership {MembershipId}",
                        membership.MembershipId);
                }
            }

            _logger.LogInformation(
                "Auto-downgrade check completed. Success: {SuccessCount}, Failures: {FailureCount}",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking expired memberships for auto-downgrade");
            throw;
        }
    }
}

