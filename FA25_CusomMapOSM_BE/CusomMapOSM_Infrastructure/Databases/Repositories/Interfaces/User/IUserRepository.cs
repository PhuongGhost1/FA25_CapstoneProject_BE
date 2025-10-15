using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Memberships;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;

public interface IUserRepository
{
    Task<DomainUser.User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<DomainUser.User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> UpdateUserAsync(DomainUser.User user, CancellationToken ct = default);
    Task<bool> UpdateUserTokenUsageAsync(Guid userId, int tokenUsage, CancellationToken ct = default);
    Task<bool> ResetUserMonthlyTokensAsync(Guid userId, CancellationToken ct = default);
    Task<MembershipUsage?> GetUserMembershipUsageAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UpdateMembershipUsageAsync(MembershipUsage usage, CancellationToken ct = default);
    Task<int> GetUserTokenUsageAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUserTotalTokensAsync(Guid userId, CancellationToken ct = default);
}
