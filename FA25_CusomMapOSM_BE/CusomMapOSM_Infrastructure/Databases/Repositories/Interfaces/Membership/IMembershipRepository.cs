using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;

public interface IMembershipRepository
{
    Task<DomainMembership?> GetByIdAsync(Guid membershipId, CancellationToken ct);
    Task<DomainMembership?> GetByUserOrgAsync(Guid userId, Guid orgId, CancellationToken ct);
    Task<DomainMembership?> GetByUserOrgWithIncludesAsync(Guid userId, Guid orgId, CancellationToken ct);
    Task<DomainMembership> UpsertAsync(DomainMembership membership, CancellationToken ct);
    Task<DomainMembershipUsage?> GetUsageAsync(Guid membershipId, Guid orgId, CancellationToken ct);
    Task<DomainMembershipUsage> UpsertUsageAsync(DomainMembershipUsage usage, CancellationToken ct);
}