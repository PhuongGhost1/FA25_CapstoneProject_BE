using OrganizationEntity = CusomMapOSM_Domain.Entities.Organizations.Organization;
using UserEntity = CusomMapOSM_Domain.Entities.Users.User;
using MembershipEntity = CusomMapOSM_Domain.Entities.Memberships.Membership;
using OrganizationMemberEntity = CusomMapOSM_Domain.Entities.Organizations.OrganizationMember;
using TransactionEntity = CusomMapOSM_Domain.Entities.Transactions.Transactions;
using CusomMapOSM_Domain.Entities.Transactions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;

public interface IOrganizationAdminRepository
{
    // Organization Management Endpoints (Org Owners/Admins) - Required Methods Only

    // Usage Data
    Task<Dictionary<string, int>> GetOrganizationUsageStatsAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetTotalActiveUsersAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetTotalMapsCreatedAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetTotalExportsThisMonthAsync(Guid orgId, CancellationToken ct = default);

    // Membership Data
    Task<List<MembershipEntity>> GetOrganizationMembershipsAsync(Guid orgId, CancellationToken ct = default);
    Task<List<MembershipEntity>> GetActiveMembershipsAsync(Guid orgId, CancellationToken ct = default);
    Task<List<MembershipEntity>> GetExpiredMembershipsAsync(Guid orgId, CancellationToken ct = default);
    Task<MembershipEntity?> GetPrimaryMembershipAsync(Guid orgId, CancellationToken ct = default);

    // Transaction Data
    Task<List<TransactionEntity>> GetOrganizationTransactionsAsync(Guid orgId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<TransactionEntity>> GetTransactionsByDateRangeAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<decimal> GetTotalSpentInPeriodAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default);

    // Organization Data
    Task<OrganizationEntity?> GetOrganizationByIdAsync(Guid orgId, CancellationToken ct = default);
    Task<bool> IsUserOrganizationAdminAsync(Guid userId, Guid orgId, CancellationToken ct = default);
    Task<bool> IsUserOrganizationOwnerAsync(Guid userId, Guid orgId, CancellationToken ct = default);
}
