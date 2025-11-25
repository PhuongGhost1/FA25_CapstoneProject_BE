using CusomMapOSM_Domain.Entities.Workspaces;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id);
    Task<IEnumerable<Workspace>> GetAllAsync();
    Task<IEnumerable<Workspace>> GetByOrganizationIdAsync(Guid orgId);
    Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId);
    Task<Workspace?> GetPersonalWorkspaceAsync(Guid userId);
    Task<Workspace> CreateAsync(Workspace workspace);
    Task<Workspace> UpdateAsync(Workspace workspace);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetMapCountAsync(Guid workspaceId);
}
