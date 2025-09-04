using DomainAccessTools = CusomMapOSM_Domain.Entities.AccessTools;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessTool;

public interface IAccessToolRepository
{
    Task<DomainAccessTools.AccessTool?> GetByIdAsync(int accessToolId, CancellationToken ct);
    Task<IReadOnlyList<DomainAccessTools.AccessTool>> GetByIdsAsync(IEnumerable<int> accessToolIds, CancellationToken ct);
    Task<IReadOnlyList<DomainAccessTools.AccessTool>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<DomainAccessTools.AccessTool>> GetByRequiredMembershipAsync(bool requiredMembership, CancellationToken ct);
}
