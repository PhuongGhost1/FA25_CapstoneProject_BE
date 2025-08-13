using CusomMapOSM_Domain.Entities.AccessTools;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;

public interface IAccessToolRepository
{
    Task<AccessTool?> GetByIdAsync(int accessToolId, CancellationToken ct);
    Task<IReadOnlyList<AccessTool>> GetByIdsAsync(IEnumerable<int> accessToolIds, CancellationToken ct);
    Task<IReadOnlyList<AccessTool>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<AccessTool>> GetByRequiredMembershipAsync(bool requiredMembership, CancellationToken ct);
}
