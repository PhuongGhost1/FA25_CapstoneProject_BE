using CusomMapOSM_Domain.Entities.Workspaces;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspace;

public interface IWorkspaceRepository
{
    Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace?> GetByIdAsync(Guid id);
    Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetAllAsync();
    Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetByOrganizationIdAsync(Guid orgId);
    Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetByUserIdAsync(Guid userId);
    Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace> CreateAsync(CusomMapOSM_Domain.Entities.Workspaces.Workspace workspace);
    Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace> UpdateAsync(CusomMapOSM_Domain.Entities.Workspaces.Workspace workspace);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetMapCountAsync(Guid workspaceId);
}
