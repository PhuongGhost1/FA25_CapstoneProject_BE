using System;
using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.SystemAdmin;

// System User Management DTOs
public record SystemUserDto
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public required string Status { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public int TotalOrganizations { get; set; }
    public int TotalActiveMemberships { get; set; }
}

public record SystemUserListResponse
{
    public required IReadOnlyList<SystemUserDto> Users { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record UpdateUserStatusRequest
{
    public required Guid UserId { get; set; }
    public required string Status { get; set; }
    public string? Reason { get; set; }
}

public record UpdateUserStatusResponse
{
    public required Guid UserId { get; set; }
    public required string OldStatus { get; set; }
    public required string NewStatus { get; set; }
    public required string Message { get; set; }
}

// System Organization Management DTOs
public record SystemOrganizationDto
{
    public required Guid OrgId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required Guid OwnerUserId { get; set; }
    public required string OwnerName { get; set; }
    public required string OwnerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalMembers { get; set; }
    public int TotalActiveMemberships { get; set; }
    public decimal TotalRevenue { get; set; }
    public string PrimaryPlanName { get; set; } = string.Empty;
}

public record SystemOrganizationListResponse
{
    public required IReadOnlyList<SystemOrganizationDto> Organizations { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record UpdateOrganizationStatusRequest
{
    public required Guid OrgId { get; set; }
    public required string Status { get; set; }
    public string? Reason { get; set; }
}

public record UpdateOrganizationStatusResponse
{
    public required Guid OrgId { get; set; }
    public required string OldStatus { get; set; }
    public required string NewStatus { get; set; }
    public required string Message { get; set; }
}

// System Subscription Plan Management DTOs
public record SystemSubscriptionPlanDto
{
    public required int PlanId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required decimal PriceMonthly { get; set; }
    public required decimal PriceYearly { get; set; }
    public required int MapsLimit { get; set; }
    public required int ExportsLimit { get; set; }
    public required int CustomLayersLimit { get; set; }
    public required int MonthlyTokenLimit { get; set; }
    public required bool IsPopular { get; set; }
    public required bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalSubscribers { get; set; }
    public decimal TotalRevenue { get; set; }
}

public record CreateSubscriptionPlanRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal PriceMonthly { get; set; }
    public required decimal PriceYearly { get; set; }
    public required int MapsLimit { get; set; }
    public required int ExportsLimit { get; set; }
    public required int CustomLayersLimit { get; set; }
    public required int MonthlyTokenLimit { get; set; }
    public bool IsPopular { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public record UpdateSubscriptionPlanRequest
{
    public required int PlanId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? PriceMonthly { get; set; }
    public decimal? PriceYearly { get; set; }
    public int? MapsLimit { get; set; }
    public int? ExportsLimit { get; set; }
    public int? CustomLayersLimit { get; set; }
    public int? MonthlyTokenLimit { get; set; }
    public bool? IsPopular { get; set; }
    public bool? IsActive { get; set; }
}

// System Support Ticket Management DTOs
public record SystemSupportTicketDto
{
    public required Guid TicketId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required string Priority { get; set; }
    public required string Category { get; set; }
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int MessageCount { get; set; }
    public string? LastMessage { get; set; }
}

public record SystemSupportTicketListResponse
{
    public required IReadOnlyList<SystemSupportTicketDto> Tickets { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record UpdateSupportTicketRequest
{
    public required Guid TicketId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Response { get; set; }
}

public record UpdateSupportTicketResponse
{
    public required Guid TicketId { get; set; }
    public required string Message { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// System Usage Monitoring DTOs
public record SystemUsageStatsDto
{
    public required DateTime GeneratedAt { get; set; }
    public required SystemUserStatsDto UserStats { get; set; }
    public required SystemOrganizationStatsDto OrganizationStats { get; set; }
    public required SystemSubscriptionStatsDto SubscriptionStats { get; set; }
    public required SystemRevenueStatsDto RevenueStats { get; set; }
    public required SystemPerformanceStatsDto PerformanceStats { get; set; }
}

public record SystemUserStatsDto
{
    public required int TotalUsers { get; set; }
    public required int ActiveUsers { get; set; }
    public required int NewUsersThisMonth { get; set; }
    public required int NewUsersLastMonth { get; set; }
    public required double UserGrowthRate { get; set; }
    public required int VerifiedUsers { get; set; }
    public required int UnverifiedUsers { get; set; }
}

public record SystemOrganizationStatsDto
{
    public required int TotalOrganizations { get; set; }
    public required int ActiveOrganizations { get; set; }
    public required int NewOrganizationsThisMonth { get; set; }
    public required int NewOrganizationsLastMonth { get; set; }
    public required double OrganizationGrowthRate { get; set; }
    public required int OrganizationsWithActiveSubscriptions { get; set; }
}

public record SystemSubscriptionStatsDto
{
    public required int TotalActiveSubscriptions { get; set; }
    public required int TotalExpiredSubscriptions { get; set; }
    public required int TotalCancelledSubscriptions { get; set; }
    public required int NewSubscriptionsThisMonth { get; set; }
    public required int NewSubscriptionsLastMonth { get; set; }
    public required double SubscriptionGrowthRate { get; set; }
    public required Dictionary<string, int> SubscriptionsByPlan { get; set; }
}

public record SystemRevenueStatsDto
{
    public required decimal TotalRevenue { get; set; }
    public required decimal RevenueThisMonth { get; set; }
    public required decimal RevenueLastMonth { get; set; }
    public required double RevenueGrowthRate { get; set; }
    public required decimal AverageRevenuePerUser { get; set; }
    public required decimal AverageRevenuePerOrganization { get; set; }
    public required Dictionary<string, decimal> RevenueByPlan { get; set; }
    public required Dictionary<string, decimal> RevenueByPaymentGateway { get; set; }
}

public record SystemPerformanceStatsDto
{
    public required double AverageResponseTime { get; set; }
    public required double SystemUptime { get; set; }
    public required int TotalApiCalls { get; set; }
    public required int SuccessfulApiCalls { get; set; }
    public required int FailedApiCalls { get; set; }
    public required double ApiSuccessRate { get; set; }
    public required Dictionary<string, int> ApiCallsByEndpoint { get; set; }
    public required Dictionary<string, int> ErrorsByType { get; set; }
}

// System Dashboard DTOs
public record SystemDashboardDto
{
    public required SystemUsageStatsDto CurrentStats { get; set; }
    public required IReadOnlyList<SystemAlertDto> ActiveAlerts { get; set; }
    public required IReadOnlyList<SystemRecentActivityDto> RecentActivities { get; set; }
    public required IReadOnlyList<SystemTopUserDto> TopUsers { get; set; }
    public required IReadOnlyList<SystemTopOrganizationDto> TopOrganizations { get; set; }
}

public record SystemAlertDto
{
    public required Guid AlertId { get; set; }
    public required string Type { get; set; }
    public required string Severity { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public record SystemRecentActivityDto
{
    public required Guid ActivityId { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public record SystemTopUserDto
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required int TotalMaps { get; set; }
    public required int TotalExports { get; set; }
    public required decimal TotalSpent { get; set; }
    public required DateTime LastActive { get; set; }
}

public record SystemTopOrganizationDto
{
    public required Guid OrgId { get; set; }
    public required string Name { get; set; }
    public required string OwnerName { get; set; }
    public required int TotalMembers { get; set; }
    public required int TotalMaps { get; set; }
    public required int TotalExports { get; set; }
    public required decimal TotalSpent { get; set; }
    public required DateTime CreatedAt { get; set; }
}
