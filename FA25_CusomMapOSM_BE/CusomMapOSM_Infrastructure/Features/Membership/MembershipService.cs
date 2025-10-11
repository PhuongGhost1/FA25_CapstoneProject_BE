using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;
using DomainMembershipAddon = CusomMapOSM_Domain.Entities.Memberships.MembershipAddon;
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
        if (existing is null)
        {
            var now = DateTime.UtcNow;
            var newMembership = new DomainMembership
            {
                MembershipId = Guid.NewGuid(),
                UserId = userId,
                OrgId = orgId,
                PlanId = planId,
                StartDate = now,
                EndDate = null,
                Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active, // Use the correct active status ID
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
            existing.PlanId = planId;
            existing.AutoRenew = autoRenew;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active;
            return Option.Some<DomainMembership, ErrorCustom.Error>(await _membershipRepository.UpsertAsync(existing, ct));
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
            // 3. Keep the same start date for billing continuity
            // 4. Update auto-renewal setting
            // 5. Reset usage cycle if upgrading to higher tier

            // Determine if this is an upgrade or downgrade
            bool isUpgrade = (newPlan.PriceMonthly ?? 0) > (currentPlan.PriceMonthly ?? 0);
            bool isDowngrade = (newPlan.PriceMonthly ?? 0) < (currentPlan.PriceMonthly ?? 0);

            // Update membership
            currentMembership.PlanId = newPlanId;
            currentMembership.AutoRenew = autoRenew;
            currentMembership.UpdatedAt = now;

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
                // Fetch addons to extend quotas if available
                var addons = await _membershipRepository.GetActiveAddonsAsync(membershipId, orgId, DateTime.UtcNow, ct);

                int GetAddonExtra(string key)
                {
                    return addons.Where(a => a.AddonKey == $"extra_{key}").Sum(a => a.Quantity ?? 0);
                }

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
        var addons = await _membershipRepository.GetActiveAddonsAsync(membershipId, orgId, DateTime.UtcNow, ct);

        // Check feature flags in usage or addons
        bool fromAddon = addons.Any(a => a.AddonKey == $"feature_{featureKey}");
        bool fromUsage = false;
        if (!string.IsNullOrWhiteSpace(usage.Match(
            some: u => u.FeatureFlags,
            none: _ => null
        )))
        {
            try
            {
                using var doc = JsonDocument.Parse(usage.Match(
                    some: u => u.FeatureFlags,
                    none: _ => null
                ));
                if (doc.RootElement.TryGetProperty(featureKey, out var val) && val.ValueKind == JsonValueKind.True)
                    fromUsage = true;
            }
            catch
            {
                // ignore invalid JSON
            }
        }
        return Option.Some<bool, ErrorCustom.Error>(fromAddon || fromUsage);
    }

    public async Task<Option<DomainMembershipAddon, ErrorCustom.Error>> AddAddonAsync(Guid membershipId, Guid orgId, string addonKey, int? quantity, bool effectiveImmediately, CancellationToken ct)
    {
        var addon = new DomainMembershipAddon
        {
            AddonId = Guid.NewGuid(),
            MembershipId = membershipId,
            OrgId = orgId,
            AddonKey = addonKey,
            Quantity = quantity,
            FeaturePayload = null,
            PurchasedAt = DateTime.UtcNow,
            EffectiveFrom = effectiveImmediately ? DateTime.UtcNow : null,
            EffectiveUntil = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Option.Some<DomainMembershipAddon, ErrorCustom.Error>(await _membershipRepository.AddAddonAsync(addon, ct));
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