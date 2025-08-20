using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;

public interface IUserAccessToolRepository
{
    Task<UserAccessTool?> GetByIdAsync(int userAccessToolId, CancellationToken ct);
    Task<IReadOnlyList<UserAccessTool>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<UserAccessTool>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
    Task<UserAccessTool?> GetByUserAndToolAsync(Guid userId, int accessToolId, CancellationToken ct);
    Task<UserAccessTool> CreateAsync(UserAccessTool userAccessTool, CancellationToken ct);
    Task<UserAccessTool> UpdateAsync(UserAccessTool userAccessTool, CancellationToken ct);
    Task<bool> DeleteAsync(int userAccessToolId, CancellationToken ct);
    Task<bool> DeleteByUserAndToolAsync(Guid userId, int accessToolId, CancellationToken ct);
    Task<bool> HasAccessAsync(Guid userId, int accessToolId, CancellationToken ct);
}
