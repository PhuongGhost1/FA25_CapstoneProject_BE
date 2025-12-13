using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases;

namespace CusomMapOSM_Infrastructure.Features.Membership;

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMembershipPlanRepository _membershipPlanRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public MembershipService(
        IMembershipRepository membershipRepository,
        IMembershipPlanRepository membershipPlanRepository,
        IOrganizationRepository organizationRepository)
    {
        _membershipRepository = membershipRepository;
        _membershipPlanRepository = membershipPlanRepository;
        _organizationRepository = organizationRepository;
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
            // New membership - create with billing cycle dates
            var now = DateTime.UtcNow;
            var newMembership = new DomainMembership
            {
                MembershipId = Guid.NewGuid(),
                UserId = userId,
                OrgId = orgId,
                PlanId = planId,
                BillingCycleStartDate = now,
                BillingCycleEndDate = now.AddDays(30), // 30-day billing cycle
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
                // On renewal, reset billing cycle dates
                existing.AutoRenew = autoRenew;
                existing.UpdatedAt = now;
                existing.Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active;
                
                // Reset billing cycle dates on renewal
                existing.BillingCycleStartDate = now;
                existing.BillingCycleEndDate = now.AddDays(30); // 30-day billing cycle

                return Option.Some<DomainMembership, ErrorCustom.Error>(await _membershipRepository.UpsertAsync(existing, ct));
            }
            else
            {
                // Different plan: Handle upgrade/downgrade (Plan 1 <-> Plan 2)
            // Determine if this is an upgrade or downgrade
            bool isUpgrade = (plan.PriceMonthly ?? 0) > (currentPlan.PriceMonthly ?? 0);
            bool isDowngrade = (plan.PriceMonthly ?? 0) < (currentPlan.PriceMonthly ?? 0);

            // Policy: Downgrades are NOT allowed during billing cycle
            // If users don't renew their subscription, it automatically downgrades to the free plan
            if (isDowngrade)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error(
                        "Membership.DowngradeNotAllowed",
                        $"Downgrades are not allowed during billing cycle. Your current plan expires on {existing.BillingCycleEndDate:yyyy-MM-dd}. If you don't renew, your plan will automatically downgrade to the free plan.",
                        ErrorCustom.ErrorType.Validation));
            }

                // For upgrade: Allow immediately
                // Update membership to new plan
                existing.PlanId = planId;
                existing.AutoRenew = autoRenew;
                existing.UpdatedAt = now;
                existing.Status = CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active;

                // For upgrade: Keep billing cycle dates unchanged (for proration)
                if (isUpgrade)
                {
                    // CRITICAL: When upgrading, KEEP the same billing cycle dates
                    // DO NOT reset BillingCycleStartDate or BillingCycleEndDate
                    // This is KEY for Option C proration - billing cycle dates remain unchanged

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

                var updatedMembership = await _membershipRepository.UpsertAsync(existing, ct);
                return Option.Some<DomainMembership, ErrorCustom.Error>(updatedMembership);
            }
        }
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> ChangeSubscriptionPlanAsync(Guid userId, Guid orgId, int newPlanId, bool autoRenew, CancellationToken ct)
    {
        try
        {
            // Get the organization to verify ownership
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Organization.NotFound", "Organization not found", ErrorCustom.ErrorType.NotFound));
            }

            // Only organization owner can change subscription plans
            if (organization.OwnerUserId != userId)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.NotOwner", "Only the organization owner can change subscription plans", ErrorCustom.ErrorType.Forbidden));
            }

            // Validate the new plan exists and is active
            var newPlan = await _membershipPlanRepository.GetPlanByIdAsync(newPlanId, ct);
            if (newPlan == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.PlanNotFound", "New plan not found or inactive", ErrorCustom.ErrorType.NotFound));
            }

            // Get current membership (organization's membership owned by the owner)
            var currentMembership = await _membershipRepository.GetByUserOrgAsync(userId, orgId, ct);
            if (currentMembership == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.NotFound", "No active membership found for organization", ErrorCustom.ErrorType.NotFound));
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

            // Policy: Downgrades are NOT allowed during billing cycle
            // If users don't renew their subscription, it automatically downgrades to the free plan
            if (isDowngrade)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error(
                        "Membership.DowngradeNotAllowed",
                        $"Downgrades are not allowed during billing cycle. Your current plan expires on {currentMembership.BillingCycleEndDate:yyyy-MM-dd}. If you don't renew, your plan will automatically downgrade to the free plan.",
                        ErrorCustom.ErrorType.Validation));
            }
            // Update membership
            currentMembership.PlanId = newPlanId;
            currentMembership.AutoRenew = autoRenew;
            currentMembership.UpdatedAt = now;

            // Use billing cycle end date for expiration checks
            // Keep billing cycle dates unchanged on upgrade (for proration)
            // Only update PlanId
            if (isUpgrade)
            {
                // CRITICAL: When upgrading, KEEP the same billing cycle dates
                // DO NOT reset BillingCycleStartDate or BillingCycleEndDate
                // This is KEY for Option C proration - billing cycle dates remain unchanged
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
            // First try to get the user's individual membership
            var membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(userId, orgId, ct);
            if (membership != null)
            {
                return Option.Some<DomainMembership, ErrorCustom.Error>(membership);
            }

            // If no individual membership found, check if user is a member of an organization
            // and return the organization's membership (owned by the organization owner)
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization != null)
            {
                // Check if the user is an active member of this organization
                var members = await _organizationRepository.GetOrganizationMembers(orgId);
                var userMember = members?.FirstOrDefault(m => m.UserId == userId && m.Status == CusomMapOSM_Domain.Entities.Organizations.Enums.MemberStatus.Active);

                if (userMember != null)
                {
                    // User is a member, get the organization's membership (owner's membership)
                    var orgMembership = await _membershipRepository.GetByUserOrgWithIncludesAsync(organization.OwnerUserId, orgId, ct);
                    if (orgMembership != null)
                    {
                        return Option.Some<DomainMembership, ErrorCustom.Error>(orgMembership);
                    }
                }
            }

            return Option.None<DomainMembership, ErrorCustom.Error>(new ErrorCustom.Error("Membership.NotFound", "No active membership found", ErrorCustom.ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            return Option.None<DomainMembership, ErrorCustom.Error>(
                new ErrorCustom.Error("Membership.GetFailed", $"Failed to get current membership: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<DomainMembership, ErrorCustom.Error>> GetMembershipByIdAsync(Guid membershipId, CancellationToken ct = default)
    {
        try
        {
            var membership = await _membershipRepository.GetByIdAsync(membershipId, ct);
            if (membership == null)
            {
                return Option.None<DomainMembership, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.NotFound", "Membership not found", ErrorCustom.ErrorType.NotFound));
            }
            return Option.Some<DomainMembership, ErrorCustom.Error>(membership);
        }
        catch (Exception ex)
        {
            return Option.None<DomainMembership, ErrorCustom.Error>(
                new ErrorCustom.Error("Membership.GetFailed", $"Failed to get membership: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<PlanLimitsResponse, ErrorCustom.Error>> GetPlanLimitsAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var plan = await _membershipPlanRepository.GetPlanByIdAsync(planId, ct);
            if (plan == null)
            {
                return Option.None<PlanLimitsResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Plan.NotFound", $"Plan with ID '{planId}' not found", ErrorCustom.ErrorType.NotFound));
            }

            static int? ToNullableInt(int value) => value == -1 ? null : value;
            static long? ToNullableLong(long value) => value == -1 ? null : value;

            var response = new PlanLimitsResponse
            {
                PlanName = plan.PlanName,
                PriceMonthly = plan.PriceMonthly ?? 0,
                OrganizationMax = null,
                LocationMax = ToNullableInt(plan.MaxLocationsPerOrg),
                ViewsMonthly = ToNullableInt(plan.MaxInteractionsPerMap),
                MapsMax = ToNullableInt(plan.MaxMapsPerMonth),
                MembersMax = ToNullableInt(plan.MaxUsersPerOrg),
                MapQuota = ToNullableInt(plan.MapQuota),
                ExportQuota = ToNullableInt(plan.ExportQuota),
                MaxLayer = ToNullableInt(plan.MaxCustomLayers),
                TokenMonthly = ToNullableInt(plan.MonthlyTokens),
                MediaFileMax = ToNullableLong(plan.MaxMediaFileSizeBytes),
                VideoFileMax = ToNullableLong(plan.MaxVideoFileSizeBytes),
                AudioFileMax = ToNullableLong(plan.MaxAudioFileSizeBytes)
            };

            return Option.Some<PlanLimitsResponse, ErrorCustom.Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<PlanLimitsResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Plan.GetLimitsFailed", $"Failed to get plan limits: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}