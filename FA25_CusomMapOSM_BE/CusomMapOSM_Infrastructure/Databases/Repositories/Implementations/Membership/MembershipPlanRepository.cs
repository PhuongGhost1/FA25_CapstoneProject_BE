using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;

public class MembershipPlanRepository : IMembershipPlanRepository
{
    private readonly CustomMapOSMDbContext _context;
    public MembershipPlanRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct)
    {
        return await _context.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceMonthly ?? 0)
            .ToListAsync(ct);
    }

    public async Task<Plan?> GetPlanByIdAsync(int planId, CancellationToken ct)
    {
        return await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == planId && p.IsActive, ct);
    }
}