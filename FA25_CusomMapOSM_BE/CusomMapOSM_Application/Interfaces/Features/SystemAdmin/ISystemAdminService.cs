using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.SystemAdmin;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.SystemAdmin;

public interface ISystemAdminService
{
    // System User Management
    Task<Option<SystemUserListResponse, Error>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default);
    Task<Option<SystemUserDto, Error>> GetUserDetailsAsync(Guid userId, CancellationToken ct = default);
    Task<Option<UpdateUserStatusResponse, Error>> UpdateUserStatusAsync(UpdateUserStatusRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteUserAsync(Guid userId, CancellationToken ct = default);
    Task<Option<bool, Error>> ImpersonateUserAsync(Guid userId, CancellationToken ct = default);

    // System Organization Management
    Task<Option<SystemOrganizationListResponse, Error>> GetAllOrganizationsAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default);
    Task<Option<SystemOrganizationDto, Error>> GetOrganizationDetailsAsync(Guid orgId, CancellationToken ct = default);
    Task<Option<UpdateOrganizationStatusResponse, Error>> UpdateOrganizationStatusAsync(UpdateOrganizationStatusRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteOrganizationAsync(Guid orgId, CancellationToken ct = default);
    Task<Option<bool, Error>> TransferOrganizationOwnershipAsync(Guid orgId, Guid newOwnerId, CancellationToken ct = default);

    // System Subscription Plan Management
    Task<Option<List<SystemSubscriptionPlanDto>, Error>> GetAllSubscriptionPlansAsync(CancellationToken ct = default);
    Task<Option<SystemSubscriptionPlanDto, Error>> GetSubscriptionPlanDetailsAsync(int planId, CancellationToken ct = default);
    Task<Option<SystemSubscriptionPlanDto, Error>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request, CancellationToken ct = default);
    Task<Option<SystemSubscriptionPlanDto, Error>> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSubscriptionPlanAsync(int planId, CancellationToken ct = default);
    Task<Option<bool, Error>> ActivateSubscriptionPlanAsync(int planId, CancellationToken ct = default);
    Task<Option<bool, Error>> DeactivateSubscriptionPlanAsync(int planId, CancellationToken ct = default);

    // System Support Ticket Management
    Task<Option<SystemSupportTicketListResponse, Error>> GetAllSupportTicketsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null, string? category = null, CancellationToken ct = default);
    Task<Option<SystemSupportTicketDto, Error>> GetSupportTicketDetailsAsync(int ticketId, CancellationToken ct = default);
    Task<Option<SystemAdminUpdateSupportTicketResponse, Error>> UpdateSupportTicketAsync(SystemAdminUpdateSupportTicketRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> CloseSupportTicketAsync(int ticketId, string resolution, CancellationToken ct = default);
    Task<Option<bool, Error>> AssignSupportTicketAsync(int ticketId, Guid assignedToUserId, CancellationToken ct = default);
    Task<Option<bool, Error>> EscalateSupportTicketAsync(int ticketId, string reason, CancellationToken ct = default);

    // System Usage Monitoring
    Task<Option<SystemUsageStatsDto, Error>> GetSystemUsageStatsAsync(CancellationToken ct = default);
    Task<Option<SystemDashboardDto, Error>> GetSystemDashboardAsync(CancellationToken ct = default);
    Task<Option<FlattenedSystemDashboardDto, Error>> GetFlattenedSystemDashboardAsync(CancellationToken ct = default);
    Task<Option<List<SystemAlertDto>, Error>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task<Option<bool, Error>> ResolveAlertAsync(Guid alertId, string resolution, CancellationToken ct = default);
    Task<Option<List<SystemRecentActivityDto>, Error>> GetRecentActivitiesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);

    // System Analytics
    Task<Option<Dictionary<string, object>, Error>> GetSystemAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Option<List<SystemTopUserDto>, Error>> GetTopUsersAsync(int count = 10, CancellationToken ct = default);
    Task<Option<List<SystemTopOrganizationDto>, Error>> GetTopOrganizationsAsync(int count = 10, CancellationToken ct = default);
    Task<Option<Dictionary<string, decimal>, Error>> GetRevenueAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Option<RevenueAnalyticsDto, Error>> GetDailyRevenueAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    // System Maintenance
    Task<Option<bool, Error>> PerformSystemMaintenanceAsync(string maintenanceType, CancellationToken ct = default);
    Task<Option<bool, Error>> ClearSystemCacheAsync(CancellationToken ct = default);
    Task<Option<bool, Error>> BackupSystemDataAsync(CancellationToken ct = default);
    Task<Option<bool, Error>> RestoreSystemDataAsync(string backupId, CancellationToken ct = default);

    // System Configuration
    Task<Option<Dictionary<string, object>, Error>> GetSystemConfigurationAsync(CancellationToken ct = default);
    Task<Option<bool, Error>> UpdateSystemConfigurationAsync(Dictionary<string, object> configuration, CancellationToken ct = default);
    Task<Option<bool, Error>> ResetSystemConfigurationAsync(CancellationToken ct = default);

    // Authorization Helpers
    Task<Option<bool, Error>> IsUserSystemAdminAsync(Guid userId, CancellationToken ct = default);
    Task<Option<bool, Error>> IsUserSuperAdminAsync(Guid userId, CancellationToken ct = default);
}
