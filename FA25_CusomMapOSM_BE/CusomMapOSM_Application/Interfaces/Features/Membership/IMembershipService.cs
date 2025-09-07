using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;
using DomainMembershipAddon = CusomMapOSM_Domain.Entities.Memberships.MembershipAddon;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;

namespace CusomMapOSM_Application.Interfaces.Features.Membership;

public interface IMembershipService
{
    Task<Option<DomainMembership, ErrorCustom.Error>> CreateOrRenewMembershipAsync(Guid userId, Guid orgId, int planId, bool autoRenew, CancellationToken ct);
    Task<Option<DomainMembership, ErrorCustom.Error>> GetMembershipAsync(Guid membershipId, CancellationToken ct);
    Task<Option<DomainMembership, ErrorCustom.Error>> GetMembershipByUserOrgAsync(Guid userId, Guid orgId, CancellationToken ct);
    Task<Option<DomainMembershipUsage, ErrorCustom.Error>> GetOrCreateUsageAsync(Guid membershipId, Guid orgId, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> TryConsumeQuotaAsync(Guid membershipId, Guid orgId, string resourceKey, int amount, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> ResetUsageCycleAsync(Guid membershipId, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> HasFeatureAsync(Guid membershipId, Guid orgId, string featureKey, CancellationToken ct);
    Task<Option<DomainMembershipAddon, ErrorCustom.Error>> AddAddonAsync(Guid membershipId, Guid orgId, string addonKey, int? quantity, bool effectiveImmediately, CancellationToken ct);

    // New method for subscription plan changes
    Task<Option<DomainMembership, ErrorCustom.Error>> ChangeSubscriptionPlanAsync(Guid userId, Guid orgId, int newPlanId, bool autoRenew, CancellationToken ct);

    // Method to get current membership with all related data
    Task<Option<DomainMembership, ErrorCustom.Error>> GetCurrentMembershipWithIncludesAsync(Guid userId, Guid orgId, CancellationToken ct);
}