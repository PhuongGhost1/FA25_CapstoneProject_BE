using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipUsage = CusomMapOSM_Domain.Entities.Memberships.MembershipUsage;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;

public class MembershipRepository : IMembershipRepository
{
    private readonly CustomMapOSMDbContext _context;

    public MembershipRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<DomainMembership?> GetByIdAsync(Guid membershipId, CancellationToken ct)
    {
        return await _context.Memberships.FirstOrDefaultAsync(m => m.MembershipId == membershipId, ct);
    }

    public async Task<DomainMembership?> GetByUserOrgAsync(Guid userId, Guid orgId, CancellationToken ct)
    {
        return await _context.Memberships
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.OrgId == orgId, ct);
    }

    public async Task<DomainMembership?> GetByUserOrgWithIncludesAsync(Guid userId, Guid orgId, CancellationToken ct)
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(m => m.Organization)
            .Include(m => m.Plan)
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.OrgId == orgId, ct);
    }

    public async Task<DomainMembership> UpsertAsync(DomainMembership membership, CancellationToken ct)
    {
        var exists = await _context.Memberships.AnyAsync(m => m.MembershipId == membership.MembershipId, ct);
        if (!exists)
        {
            await _context.Memberships.AddAsync(membership, ct);
        }
        else
        {
            _context.Memberships.Update(membership);
        }
        await _context.SaveChangesAsync(ct);
        return membership;
    }

    public async Task<DomainMembershipUsage?> GetUsageAsync(Guid membershipId, Guid orgId, CancellationToken ct)
    {
        return await _context.MembershipUsages.FirstOrDefaultAsync(u => u.MembershipId == membershipId && u.OrgId == orgId, ct);
    }

    public async Task<DomainMembershipUsage> UpsertUsageAsync(DomainMembershipUsage usage, CancellationToken ct)
    {
        var exists = await _context.MembershipUsages.AnyAsync(u => u.UsageId == usage.UsageId, ct);
        if (!exists)
        {
            await _context.MembershipUsages.AddAsync(usage, ct);
        }
        else
        {
            _context.MembershipUsages.Update(usage);
        }
        await _context.SaveChangesAsync(ct);
        return usage;
    }

}