using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Microsoft.EntityFrameworkCore;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases;

namespace CusomMapOSM_Infrastructure.Features.Membership;

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMembershipPlanRepository _membershipPlanRepository;

    public MembershipService(
        IMembershipRepository membershipRepository,
        IMembershipPlanRepository membershipPlanRepository)
    {
        _membershipRepository = membershipRepository;
        _membershipPlanRepository = membershipPlanRepository;
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> CreateOrRenewMembershipAsync(Guid userId, Guid orgId, int planId, bool autoRenew, CancellationToken ct)
    {
        var existing = await _membershipRepository.GetByUserOrgAsync(userId, orgId, ct);
        var plan = await _membershipPlanRepository.GetPlanByIdAsync(planId, ct);
        if (plan == null)
        {
            return Option.None<DomainMembership, ErrorCustom.Error>(
                new ErrorCustom.Error("Membership.PlanNotFound", "Plan not found", ErrorCustom.ErrorType.NotFound));
        }

        if (existing is null)
        {
            // New membership - create with EndDate based on plan duration
            var now = DateTime.UtcNow;
            var newMembership = new DomainMembership
            {
                MembershipId = Guid.NewGuid(),
                UserId = userId,
                OrgId = orgId,
                PlanId = planId,
                StartDate = now,
                EndDate = now.AddMonths(plan.DurationMonths), // Set EndDate based on plan duration
                Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active,
                AutoRenew = autoRenew,
                CurrentUsage = null,
                LastResetDate = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            newMembership = await _membershipRepository.UpsertAsync(newMembership, ct);

            // Initialize usage row per org
            var usage = new DomainMembershipUsage
            {
                UsageId = Guid.NewGuid(),
                MembershipId = newMembership.MembershipId,
                OrgId = orgId,
                MapsCreatedThisCycle = 0,
                ExportsThisCycle = 0,
                ActiveUsersInOrg = 0,
                FeatureFlags = null,
                CycleStartDate = now,
                CycleEndDate = now.AddMonths(1),
                CreatedAt = now,
                UpdatedAt = now
            };
            await _membershipRepository.UpsertUsageAsync(usage, ct);

            return Option.Some<DomainMembership, ErrorCustom.Error>(newMembership);
        }
        else
        {
            // Existing membership - check if same plan or different plan
            var currentPlan = await _membershipPlanRepository.GetPlanByIdAsync(existing.PlanId, ct);
            if (currentPlan == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.CurrentPlanNotFound", "Current plan not found", ErrorCustom.ErrorType.NotFound));
            }

            var now = DateTime.UtcNow;

            // If same plan: extend subscription time (only time is different)
            if (existing.PlanId == planId)
            {
                // Get the current end date or start from now if expired
                var currentEndDate = existing.EndDate ?? now;
                if (currentEndDate < now)
                {
                    currentEndDate = now; // If expired, start from now
                }

                // Extend subscription by adding plan duration months
                existing.EndDate = currentEndDate.AddMonths(plan.DurationMonths);
                existing.AutoRenew = autoRenew;
                existing.UpdatedAt = now;
                existing.Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active;

                return Option.Some<DomainMembership, ErrorCustom.Error>(await _membershipRepository.UpsertAsync(existing, ct));
            }
            else
            {
                // Different plan: Handle upgrade/downgrade (Plan 1 <-> Plan 2)
                // Determine if this is an upgrade or downgrade
                bool isUpgrade = (plan.PriceMonthly ?? 0) > (currentPlan.PriceMonthly ?? 0);
                bool isDowngrade = (plan.PriceMonthly ?? 0) < (currentPlan.PriceMonthly ?? 0);

                // Calculate remaining time in the current subscription
                var currentEndDate = existing.EndDate ?? now;
                if (currentEndDate < now)
                {
                    currentEndDate = now; // If expired, treat as 0 remaining time
                }

                var remainingDays = Math.Max(0, (currentEndDate - now).Days);

                // For downgrade: Only allow if within 7 days of expiration
                if (isDowngrade && remainingDays > 7)
                {
                    // User must wait until subscription expires or is within 7 days of expiration
                    var daysUntilCanDowngrade = remainingDays - 7;
                    return Option.None<DomainMembership, ErrorCustom.Error>(
                        new ErrorCustom.Error(
                            "Membership.Downgrade.NotAllowed",
                            $"Cannot downgrade yet. Your current plan expires in {remainingDays} days. Please wait until {daysUntilCanDowngrade} more days (7 days before expiration) or until your subscription expires.",
                            ErrorCustom.ErrorType.Validation));
                }

                // For upgrade: Allow immediately
                // Update membership to new plan
                existing.PlanId = planId;
                existing.AutoRenew = autoRenew;
                existing.UpdatedAt = now;
                existing.Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active;

                // Calculate new EndDate based on plan type
                if (isUpgrade)
                {
                    // For upgrade: extend subscription from current end date by new plan duration
                    existing.EndDate = currentEndDate.AddMonths(plan.DurationMonths);

                    // Reset usage cycle to give immediate access to higher quotas
                    existing.LastResetDate = now;
                    var usage = await _membershipRepository.GetUsageAsync(existing.MembershipId, orgId, ct);
                    if (usage != null)
                    {
                        usage.MapsCreatedThisCycle = 0;
                        usage.ExportsThisCycle = 0;
                        usage.ActiveUsersInOrg = 0;
                        usage.CycleStartDate = now;
                        usage.CycleEndDate = now.AddMonths(1);
                        usage.UpdatedAt = now;
                        await _membershipRepository.UpsertUsageAsync(usage, ct);
                    }
                }
                else if (isDowngrade)
                {
                    // For downgrade: Keep current end date (user keeps time they paid for)
                    existing.EndDate = currentEndDate;

                    // Cap usage to new plan limits
                    var usage = await _membershipRepository.GetUsageAsync(existing.MembershipId, orgId, ct);
                    if (usage != null)
                    {
                        if (plan.MaxMapsPerMonth > 0 && usage.MapsCreatedThisCycle > plan.MaxMapsPerMonth)
                        {
                            usage.MapsCreatedThisCycle = plan.MaxMapsPerMonth;
                        }
                        if (plan.ExportQuota > 0 && usage.ExportsThisCycle > plan.ExportQuota)
                        {
                            usage.ExportsThisCycle = plan.ExportQuota;
                        }
                        if (plan.MaxUsersPerOrg > 0 && usage.ActiveUsersInOrg > plan.MaxUsersPerOrg)
                        {
                            usage.ActiveUsersInOrg = plan.MaxUsersPerOrg;
                        }
                        usage.UpdatedAt = now;
                        await _membershipRepository.UpsertUsageAsync(usage, ct);
                    }
                }

                var updatedMembership = await _membershipRepository.UpsertAsync(existing, ct);
                return Option.Some<DomainMembership, ErrorCustom.Error>(updatedMembership);
            }
        }
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> ChangeSubscriptionPlanAsync(Guid userId, Guid orgId, int newPlanId, bool autoRenew, CancellationToken ct)
    {
        try
        {
            // Validate the new plan exists and is active
            var newPlan = await _membershipPlanRepository.GetPlanByIdAsync(newPlanId, ct);
            if (newPlan == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.PlanNotFound", "New plan not found or inactive", ErrorCustom.ErrorType.NotFound));
            }

            // Get current membership
            var currentMembership = await _membershipRepository.GetByUserOrgAsync(userId, orgId, ct);
            if (currentMembership == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.NotFound", "No active membership found for user and organization", ErrorCustom.ErrorType.NotFound));
            }

            // Check if it's the same plan
            if (currentMembership.PlanId == newPlanId)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.SamePlan", "Cannot change to the same plan", ErrorCustom.ErrorType.Validation));
            }

            // Get current plan for comparison
            var currentPlan = await _membershipPlanRepository.GetPlanByIdAsync(currentMembership.PlanId, ct);
            if (currentPlan == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.CurrentPlanNotFound", "Current plan not found", ErrorCustom.ErrorType.NotFound));
            }

            var now = DateTime.UtcNow;

            // Business Rules for Plan Changes:
            // 1. Allow immediate plan changes
            // 2. Update membership with new plan
            // 3. Calculate price difference based on remaining time
            // 4. Update auto-renewal setting
            // 5. Reset usage cycle if upgrading to higher tier

            // Determine if this is an upgrade or downgrade
            bool isUpgrade = (newPlan.PriceMonthly ?? 0) > (currentPlan.PriceMonthly ?? 0);
            bool isDowngrade = (newPlan.PriceMonthly ?? 0) < (currentPlan.PriceMonthly ?? 0);

            // Calculate remaining time in the current subscription
            var currentEndDate = currentMembership.EndDate ?? now;
            if (currentEndDate < now)
            {
                currentEndDate = now; // If expired, treat as 0 remaining time
            }

            var remainingDays = Math.Max(0, (currentEndDate - now).Days);

            // For downgrade: Only allow if within 7 days of expiration
            if (isDowngrade && remainingDays > 7)
            {
                // User must wait until subscription expires or is within 7 days of expiration
                var daysUntilCanDowngrade = remainingDays - 7;
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error(
                        "Membership.Downgrade.NotAllowed",
                        $"Cannot downgrade yet. Your current plan expires in {remainingDays} days. Please wait until {daysUntilCanDowngrade} more days (7 days before expiration) or until your subscription expires.",
                        ErrorCustom.ErrorType.Validation));
            }
            var totalDaysInCurrentCycle = currentPlan.DurationMonths * 30; // Approximate: using 30 days per month
            var remainingMonthsRatio = totalDaysInCurrentCycle > 0
                ? (decimal)remainingDays / (decimal)totalDaysInCurrentCycle
                : 0;

            // Update membership
            currentMembership.PlanId = newPlanId;
            currentMembership.AutoRenew = autoRenew;
            currentMembership.UpdatedAt = now;

            // Calculate new EndDate based on remaining time and new plan duration
            // If upgrade: extend time based on new plan duration
            // If downgrade: calculate remaining time proportionally or keep current end date
            if (isUpgrade)
            {
                // For upgrade: extend subscription from current end date by new plan duration
                currentMembership.EndDate = currentEndDate.AddMonths(newPlan.DurationMonths);
            }
            else if (isDowngrade)
            {
                // For downgrade: calculate proportional remaining time or keep current end date
                // Option 1: Keep current end date (simpler, user keeps time they paid for)
                // Option 2: Calculate proportionally (more complex, adjusts based on price ratio)
                // Using Option 1 for simplicity - user keeps their paid time
                currentMembership.EndDate = currentEndDate;
            }
            else
            {
                // Same price tier - extend from current end date
                currentMembership.EndDate = currentEndDate.AddMonths(newPlan.DurationMonths);
            }

            // If upgrading, reset usage cycle to give immediate access to higher quotas
            if (isUpgrade)
            {
                currentMembership.LastResetDate = now;

                // Reset usage for the organization
                var usage = await _membershipRepository.GetUsageAsync(currentMembership.MembershipId, orgId, ct);
                if (usage != null)
                {
                    usage.MapsCreatedThisCycle = 0;
                    usage.ExportsThisCycle = 0;
                    usage.ActiveUsersInOrg = 0;
                    usage.CycleStartDate = now;
                    usage.CycleEndDate = now.AddMonths(1);
                    usage.UpdatedAt = now;
                    await _membershipRepository.UpsertUsageAsync(usage, ct);
                }
            }

            // If downgrading, keep current usage but ensure it doesn't exceed new plan limits
            if (isDowngrade)
            {
                var usage = await _membershipRepository.GetUsageAsync(currentMembership.MembershipId, orgId, ct);
                if (usage != null)
                {
                    // Cap usage to new plan limits
                    if (newPlan.MaxMapsPerMonth > 0 && usage.MapsCreatedThisCycle > newPlan.MaxMapsPerMonth)
                    {
                        usage.MapsCreatedThisCycle = newPlan.MaxMapsPerMonth;
                    }
                    if (newPlan.ExportQuota > 0 && usage.ExportsThisCycle > newPlan.ExportQuota)
                    {
                        usage.ExportsThisCycle = newPlan.ExportQuota;
                    }
                    if (newPlan.MaxUsersPerOrg > 0 && usage.ActiveUsersInOrg > newPlan.MaxUsersPerOrg)
                    {
                        usage.ActiveUsersInOrg = newPlan.MaxUsersPerOrg;
                    }
                    usage.UpdatedAt = now;
                    await _membershipRepository.UpsertUsageAsync(usage, ct);
                }
            }

            var updatedMembership = await _membershipRepository.UpsertAsync(currentMembership, ct);
            return Option.Some<DomainMembership, ErrorCustom.Error>(updatedMembership);
        }
        catch (Exception ex)
        {
            return Option.None<DomainMembership, ErrorCustom.Error>(
                new ErrorCustom.Error("Membership.ChangePlanFailed", $"Failed to change subscription plan: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> GetMembershipAsync(Guid membershipId, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByIdAsync(membershipId, ct);
        return membership != null
            ? Option.Some<DomainMembership, ErrorCustom.Error>(membership)
            : Option.None<DomainMembership, ErrorCustom.Error>(new ErrorCustom.Error("Membership.NotFound", "Membership not found", ErrorCustom.ErrorType.NotFound));
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> GetMembershipByUserOrgAsync(Guid userId, Guid orgId, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByUserOrgAsync(userId, orgId, ct);
        return membership != null
            ? Option.Some<DomainMembership, ErrorCustom.Error>(membership)
            : Option.None<DomainMembership, ErrorCustom.Error>(new ErrorCustom.Error("Membership.NotFound", "Membership not found for user and organization", ErrorCustom.ErrorType.NotFound));
    }

    public async Task<Option<DomainMembershipUsage, ErrorCustom.Error>> GetOrCreateUsageAsync(Guid membershipId, Guid orgId, CancellationToken ct)
    {
        var usage = await _membershipRepository.GetUsageAsync(membershipId, orgId, ct);
        if (usage is not null)
            return Option.Some<DomainMembershipUsage, ErrorCustom.Error>(usage);

        var now = DateTime.UtcNow;
        var newUsage = new DomainMembershipUsage
        {
            UsageId = Guid.NewGuid(),
            MembershipId = membershipId,
            OrgId = orgId,
            MapsCreatedThisCycle = 0,
            ExportsThisCycle = 0,
            ActiveUsersInOrg = 0,
            FeatureFlags = null,
            CycleStartDate = now,
            CycleEndDate = now.AddMonths(1),
            CreatedAt = now,
            UpdatedAt = now,
        };
        return Option.Some<DomainMembershipUsage, ErrorCustom.Error>(await _membershipRepository.UpsertUsageAsync(newUsage, ct));
    }

    public async Task<Option<bool, ErrorCustom.Error>> TryConsumeQuotaAsync(Guid membershipId, Guid orgId, string resourceKey, int amount, CancellationToken ct)
    {
        var usageResult = await GetOrCreateUsageAsync(membershipId, orgId, ct);

        return await usageResult.Match(
            some: async usage =>
            {

                switch (resourceKey)
                {
                    case "maps":
                        usage.MapsCreatedThisCycle += amount;
                        break;
                    case "exports":
                        usage.ExportsThisCycle += amount;
                        break;
                    case "users":
                        usage.ActiveUsersInOrg += amount;
                        break;
                    default:
                        return Option.None<bool, ErrorCustom.Error>(new ErrorCustom.Error("Membership.Usage.InvalidResourceKey", "Invalid resource key", ErrorCustom.ErrorType.Validation));
                }

                usage.UpdatedAt = DateTime.UtcNow;
                await _membershipRepository.UpsertUsageAsync(usage, ct);
                return Option.Some<bool, ErrorCustom.Error>(true);
            },
            none: err => Task.FromResult(Option.None<bool, ErrorCustom.Error>(err))
        );
    }

    public async Task<Option<bool, ErrorCustom.Error>> ResetUsageCycleAsync(Guid membershipId, CancellationToken ct)
    {
        // For all usages tied to this membership, reset counters and advance cycle
        var now = DateTime.UtcNow;
        // Ideally a repo method to batch reset; to keep scope, we fetch and update if exists per org when requested by endpoint/job
        // This method remains a placeholder for a background job implementation
        await Task.CompletedTask;
        return Option.Some<bool, ErrorCustom.Error>(true);
    }

    public async Task<Option<bool, ErrorCustom.Error>> HasFeatureAsync(Guid membershipId, Guid orgId, string featureKey, CancellationToken ct)
    {
        var usage = await GetOrCreateUsageAsync(membershipId, orgId, ct);
        // Check feature flags in usage
        bool fromUsage = false;
        if (!string.IsNullOrWhiteSpace(usage.Match(
            some: u => u.FeatureFlags,
            none: _ => null
        )))
        {
            try
            {
                var featureFlagsJson = usage.Match(
                    some: u => u.FeatureFlags,
                    none: _ => null);
                if (string.IsNullOrWhiteSpace(featureFlagsJson))
                    return Option.Some<bool, ErrorCustom.Error>(false);

                using var doc = JsonDocument.Parse(featureFlagsJson);
                if (doc.RootElement.TryGetProperty(featureKey, out var val) && val.ValueKind == JsonValueKind.True)
                    fromUsage = true;
            }
            catch
            {
                // ignore invalid JSON
            }
        }
        return Option.Some<bool, ErrorCustom.Error>(fromUsage);
    }


    public async Task<Option<DomainMembership, ErrorCustom.Error>> GetCurrentMembershipWithIncludesAsync(Guid userId, Guid orgId, CancellationToken ct)
    {
        try
        {
            var membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(userId, orgId, ct);
            return membership != null
                ? Option.Some<DomainMembership, ErrorCustom.Error>(membership)
                : Option.None<DomainMembership, ErrorCustom.Error>(new ErrorCustom.Error("Membership.NotFound", "No active membership found for user and organization", ErrorCustom.ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            return Option.None<DomainMembership, ErrorCustom.Error>(
                new ErrorCustom.Error("Membership.GetFailed", $"Failed to get current membership: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}