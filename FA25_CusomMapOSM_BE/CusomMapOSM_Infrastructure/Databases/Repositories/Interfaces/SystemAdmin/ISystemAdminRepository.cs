using CusomMapOSM_Application.Models.DTOs.Features.SystemAdmin;
using UserEntity = CusomMapOSM_Domain.Entities.Users.User;
using OrganizationEntity = CusomMapOSM_Domain.Entities.Organizations.Organization;
using MembershipEntity = CusomMapOSM_Domain.Entities.Memberships.Membership;
using TransactionEntity = CusomMapOSM_Domain.Entities.Transactions.Transactions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SystemAdmin;

public interface ISystemAdminRepository
{
    // User Management
    Task<List<UserEntity>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default);
    Task<int> GetTotalUsersCountAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UpdateUserStatusAsync(Guid userId, string status, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<UserEntity>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<int> GetActiveUsersCountAsync(CancellationToken ct = default);
    Task<int> GetActiveUsersTodayCountAsync(CancellationToken ct = default);
    Task<int> GetVerifiedUsersCountAsync(CancellationToken ct = default);
    Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    // Organization Management
    Task<List<OrganizationEntity>> GetAllOrganizationsAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default);
    Task<int> GetTotalOrganizationsCountAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<OrganizationEntity?> GetOrganizationByIdAsync(Guid orgId, CancellationToken ct = default);
    Task<bool> UpdateOrganizationStatusAsync(Guid orgId, string status, CancellationToken ct = default);
    Task<bool> DeleteOrganizationAsync(Guid orgId, CancellationToken ct = default);
    Task<bool> TransferOrganizationOwnershipAsync(Guid orgId, Guid newOwnerId, CancellationToken ct = default);
    Task<List<OrganizationEntity>> GetOrganizationsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<int> GetActiveOrganizationsCountAsync(CancellationToken ct = default);
    Task<int> GetOrganizationsWithActiveSubscriptionsCountAsync(CancellationToken ct = default);

    // Subscription Plan Management
    Task<List<CusomMapOSM_Domain.Entities.Memberships.Plan>> GetAllSubscriptionPlansAsync(CancellationToken ct = default);
    Task<CusomMapOSM_Domain.Entities.Memberships.Plan?> GetSubscriptionPlanByIdAsync(int planId, CancellationToken ct = default);
    Task<bool> CreateSubscriptionPlanAsync(CusomMapOSM_Domain.Entities.Memberships.Plan plan, CancellationToken ct = default);
    Task<bool> UpdateSubscriptionPlanAsync(CusomMapOSM_Domain.Entities.Memberships.Plan plan, CancellationToken ct = default);
    Task<bool> DeleteSubscriptionPlanAsync(int planId, CancellationToken ct = default);
    Task<bool> ActivateSubscriptionPlanAsync(int planId, CancellationToken ct = default);
    Task<bool> DeactivateSubscriptionPlanAsync(int planId, CancellationToken ct = default);
    Task<int> GetSubscribersCountByPlanAsync(int planId, CancellationToken ct = default);
    Task<decimal> GetRevenueByPlanAsync(int planId, CancellationToken ct = default);

    // Membership Management
    Task<List<MembershipEntity>> GetAllMembershipsAsync(CancellationToken ct = default);
    Task<List<MembershipEntity>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<List<MembershipEntity>> GetExpiredMembershipsAsync(CancellationToken ct = default);
    Task<List<MembershipEntity>> GetCancelledMembershipsAsync(CancellationToken ct = default);
    Task<List<MembershipEntity>> GetMembershipsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<int> GetNewMembershipsCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetMembershipsByPlanAsync(CancellationToken ct = default);

    // Transaction Management
    Task<List<TransactionEntity>> GetAllTransactionsAsync(CancellationToken ct = default);
    Task<List<TransactionEntity>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Dictionary<string, decimal>> GetRevenueByPaymentGatewayAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Dictionary<string, decimal>> GetRevenueByPlanAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    // Usage Statistics
    Task<Dictionary<string, int>> GetSystemUsageStatsAsync(CancellationToken ct = default);
    Task<int> GetTotalMapsCreatedAsync(CancellationToken ct = default);
    Task<int> GetTotalExportsAsync(CancellationToken ct = default);
    Task<int> GetTotalCustomLayersAsync(CancellationToken ct = default);
    Task<int> GetTotalTokensUsedAsync(CancellationToken ct = default);

    // Support Ticket Management (would need to implement support ticket entities)
    Task<List<object>> GetAllSupportTicketsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null, string? category = null, CancellationToken ct = default);
    Task<int> GetTotalSupportTicketsCountAsync(string? status = null, string? priority = null, string? category = null, CancellationToken ct = default);
    Task<object?> GetSupportTicketByIdAsync(int ticketId, CancellationToken ct = default);
    Task<bool> UpdateSupportTicketAsync(int ticketId, Dictionary<string, object> updates, CancellationToken ct = default);
    Task<bool> CloseSupportTicketAsync(int ticketId, string resolution, CancellationToken ct = default);
    Task<bool> AssignSupportTicketAsync(int ticketId, Guid assignedToUserId, CancellationToken ct = default);
    Task<bool> EscalateSupportTicketAsync(int ticketId, string reason, CancellationToken ct = default);

    // System Analytics
    Task<Dictionary<string, object>> GetSystemAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<List<UserEntity>> GetTopUsersByActivityAsync(int count = 10, CancellationToken ct = default);
    Task<List<OrganizationEntity>> GetTopOrganizationsByActivityAsync(int count = 10, CancellationToken ct = default);
    Task<List<UserEntity>> GetTopUsersByRevenueAsync(int count = 10, CancellationToken ct = default);
    Task<List<OrganizationEntity>> GetTopOrganizationsByRevenueAsync(int count = 10, CancellationToken ct = default);

    // System Configuration
    Task<Dictionary<string, object>> GetSystemConfigurationAsync(CancellationToken ct = default);
    Task<bool> UpdateSystemConfigurationAsync(Dictionary<string, object> configuration, CancellationToken ct = default);
    Task<bool> ResetSystemConfigurationAsync(CancellationToken ct = default);

    // System Maintenance
    Task<bool> ClearSystemCacheAsync(CancellationToken ct = default);
    Task<bool> BackupSystemDataAsync(CancellationToken ct = default);
    Task<bool> RestoreSystemDataAsync(string backupId, CancellationToken ct = default);
    Task<bool> PerformSystemMaintenanceAsync(string maintenanceType, CancellationToken ct = default);

    // Additional helper methods for statistics without navigation properties
    Task<int> GetUserActiveMembershipsCountAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUserTotalOrganizationsCountAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetOrganizationMembersCountAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetOrganizationActiveMembershipsCountAsync(Guid orgId, CancellationToken ct = default);
    Task<decimal> GetOrganizationTotalRevenueAsync(Guid orgId, CancellationToken ct = default);
    Task<string> GetOrganizationPrimaryPlanNameAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetUserTotalMapsAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUserTotalExportsAsync(Guid userId, CancellationToken ct = default);
    Task<decimal> GetUserTotalSpentAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetOrganizationTotalMapsAsync(Guid orgId, CancellationToken ct = default);
    Task<int> GetOrganizationTotalExportsAsync(Guid orgId, CancellationToken ct = default);
    Task<decimal> GetOrganizationTotalSpentAsync(Guid orgId, CancellationToken ct = default);
    
    // Organization detail methods
    Task<List<CusomMapOSM_Domain.Entities.Organizations.OrganizationMember>> GetOrganizationMembersAsync(Guid orgId, CancellationToken ct = default);
    Task<List<MembershipEntity>> GetOrganizationMembershipsAsync(Guid orgId, CancellationToken ct = default);
    Task<decimal> GetOrganizationTotalStorageUsedMBAsync(Guid orgId, CancellationToken ct = default);
    Task<List<TransactionEntity>> GetOrganizationRecentTransactionsAsync(Guid orgId, int count = 10, CancellationToken ct = default);
}
