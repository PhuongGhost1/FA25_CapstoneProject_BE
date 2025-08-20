using CusomMapOSM_Domain.Entities.AccessTools;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.AccessToolRepo;

public class AccessToolRepository : IAccessToolRepository
{
    private readonly CustomMapOSMDbContext _context;

    public AccessToolRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<AccessTool?> GetByIdAsync(int accessToolId, CancellationToken ct)
    {
        return await _context.AccessTools.FirstOrDefaultAsync(at => at.AccessToolId == accessToolId, ct);
    }

    public async Task<IReadOnlyList<AccessTool>> GetByIdsAsync(IEnumerable<int> accessToolIds, CancellationToken ct)
    {
        return await _context.AccessTools
            .Where(at => accessToolIds.Contains(at.AccessToolId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AccessTool>> GetAllAsync(CancellationToken ct)
    {
        return await _context.AccessTools.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AccessTool>> GetByRequiredMembershipAsync(bool requiredMembership, CancellationToken ct)
    {
        return await _context.AccessTools
            .Where(at => at.RequiredMembership == requiredMembership)
            .ToListAsync(ct);
    }
}
