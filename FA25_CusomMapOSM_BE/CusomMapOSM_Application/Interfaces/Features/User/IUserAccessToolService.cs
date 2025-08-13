using CusomMapOSM_Domain.Entities.Users;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;

namespace CusomMapOSM_Application.Interfaces.Features.User;

public interface IUserAccessToolService
{
    Task<Option<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>> GetUserAccessToolsAsync(Guid userId, CancellationToken ct);
    Task<Option<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>> GetActiveUserAccessToolsAsync(Guid userId, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> HasAccessToToolAsync(Guid userId, int accessToolId, CancellationToken ct);
    Task<Option<UserAccessTool, ErrorCustom.Error>> GrantAccessToToolAsync(Guid userId, int accessToolId, DateTime expiredAt, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> RevokeAccessToToolAsync(Guid userId, int accessToolId, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> GrantAccessToToolsAsync(Guid userId, IEnumerable<int> accessToolIds, DateTime expiredAt, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> RevokeAllAccessToolsAsync(Guid userId, CancellationToken ct);
    Task<Option<bool, ErrorCustom.Error>> UpdateAccessToolsForMembershipAsync(Guid userId, int planId, DateTime membershipExpiryDate, CancellationToken ct);
}
