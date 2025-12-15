using CusomMapOSM_Application.Interfaces.Features.SystemAdmin;
using CusomMapOSM_Application.Interfaces.Features.Exports;
using CusomMapOSM_Application.Models.DTOs.Features.SystemAdmin;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Optional.Unsafe;

namespace CusomMapOSM_API.Endpoints.SystemAdmin;

public class SystemAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags(Tags.SystemAdmin)
            .RequireAuthorization();

        // System User Management
        group.MapGet("/users", GetAllUsersBySystemAdmin)
            .WithName("GetAllUsersBySystemAdmin")
            .WithSummary("Get all users")
            .WithDescription("Retrieve paginated list of all users in the system with search and filter options");

        group.MapGet("/users/{userId:guid}", GetUserDetailsBySystemAdmin)
            .WithName("GetUserDetailsBySystemAdmin")
            .WithSummary("Get user details")
            .WithDescription("Retrieve detailed information about a specific user");

        group.MapPut("/users/{userId:guid}/status", UpdateUserStatusBySystemAdmin)
            .WithName("UpdateUserStatusBySystemAdmin")
            .WithSummary("Update user status")
            .WithDescription("Update the status of a specific user (active, inactive, suspended, etc.)");

        group.MapDelete("/users/{userId:guid}", DeleteUserBySystemAdmin)
            .WithName("DeleteUserBySystemAdmin")
            .WithSummary("Delete user")
            .WithDescription("Permanently delete a user from the system");

        // group.MapPost("/users/{userId:guid}/impersonate", ImpersonateUserBySystemAdmin)
        //     .WithName("ImpersonateUserBySystemAdmin")
        //     .WithSummary("Impersonate user")
        //     .WithDescription("Impersonate a user for support purposes (super admin only)");

        // System Organization Management
        group.MapGet("/organizations", GetAllOrganizationsBySystemAdmin)
            .WithName("GetAllOrganizationsBySystemAdmin")
            .WithSummary("Get all organizations")
            .WithDescription("Retrieve paginated list of all organizations in the system with search and filter options");

        group.MapGet("/organizations/{orgId:guid}", GetOrganizationDetailsBySystemAdmin)
            .WithName("GetOrganizationDetailsBySystemAdmin")
            .WithSummary("Get organization details")
            .WithDescription("Retrieve detailed information about a specific organization");

        group.MapPut("/organizations/{orgId:guid}/status", UpdateOrganizationStatusBySystemAdmin)
            .WithName("UpdateOrganizationStatusBySystemAdmin")
            .WithSummary("Update organization status")
            .WithDescription("Update the status of a specific organization (active, inactive, suspended, etc.)");

        group.MapDelete("/organizations/{orgId:guid}", DeleteOrganizationBySystemAdmin)
            .WithName("DeleteOrganizationBySystemAdmin")
            .WithSummary("Delete organization")
            .WithDescription("Permanently delete an organization from the system");

        // group.MapPost("/organizations/{orgId:guid}/transfer-ownership", TransferOrganizationOwnershipBySystemAdmin)
        //     .WithName("TransferOrganizationOwnershipBySystemAdmin")
        //     .WithSummary("Transfer organization ownership")
        //     .WithDescription("Transfer ownership of an organization to another user");

        // System Subscription Plan Management
        group.MapGet("/subscription-plans", GetAllSubscriptionPlansBySystemAdmin)
            .WithName("GetAllSubscriptionPlansBySystemAdmin")
            .WithSummary("Get all subscription plans")
            .WithDescription("Retrieve all subscription plans with their statistics");

        group.MapGet("/subscription-plans/{planId:int}", GetSubscriptionPlanDetailsBySystemAdmin)
            .WithName("GetSubscriptionPlanDetailsBySystemAdmin")
            .WithSummary("Get subscription plan details")
            .WithDescription("Retrieve detailed information about a specific subscription plan");

        group.MapPost("/subscription-plans", CreateSubscriptionPlanBySystemAdmin)
            .WithName("CreateSubscriptionPlanBySystemAdmin")
            .WithSummary("Create subscription plan")
            .WithDescription("Create a new subscription plan");

        group.MapPut("/subscription-plans/{planId:int}", UpdateSubscriptionPlanBySystemAdmin)
            .WithName("UpdateSubscriptionPlanBySystemAdmin")
            .WithSummary("Update subscription plan")
            .WithDescription("Update an existing subscription plan");

        group.MapDelete("/subscription-plans/{planId:int}", DeleteSubscriptionPlanBySystemAdmin)
            .WithName("DeleteSubscriptionPlanBySystemAdmin")
            .WithSummary("Delete subscription plan")
            .WithDescription("Delete a subscription plan (only if no active subscriptions)");

        group.MapPost("/subscription-plans/{planId:int}/activate", ActivateSubscriptionPlanBySystemAdmin)
            .WithName("ActivateSubscriptionPlanBySystemAdmin")
            .WithSummary("Activate subscription plan")
            .WithDescription("Activate a subscription plan");

        group.MapPost("/subscription-plans/{planId:int}/deactivate", DeactivateSubscriptionPlanBySystemAdmin)
            .WithName("DeactivateSubscriptionPlanBySystemAdmin")
            .WithSummary("Deactivate subscription plan")
            .WithDescription("Deactivate a subscription plan");

        // System Support Ticket Management
        group.MapGet("/support-tickets", GetAllSupportTicketsBySystemAdmin)
            .WithName("GetAllSupportTicketsBySystemAdmin")
            .WithSummary("Get all support tickets")
            .WithDescription("Retrieve paginated list of all support tickets with filter options");

        group.MapGet("/support-tickets/{ticketId:int}", GetSupportTicketDetailsBySystemAdmin)
            .WithName("GetSupportTicketDetailsBySystemAdmin")
            .WithSummary("Get support ticket details")
            .WithDescription("Retrieve detailed information about a specific support ticket");

        group.MapPut("/support-tickets/{ticketId:int}", UpdateSupportTicketBySystemAdmin)
            .WithName("UpdateSupportTicketBySystemAdmin")
            .WithSummary("Update support ticket")
            .WithDescription("Update a support ticket (status, priority, assignment, etc.)");

        group.MapPost("/support-tickets/{ticketId:int}/close", CloseSupportTicketBySystemAdmin)
            .WithName("CloseSupportTicketBySystemAdmin")
            .WithSummary("Close support ticket")
            .WithDescription("Close a support ticket with resolution");

        // group.MapPost("/support-tickets/{ticketId:int}/assign", AssignSupportTicketBySystemAdmin)
        //     .WithName("AssignSupportTicketBySystemAdmin")
        //     .WithSummary("Assign support ticket")
        //     .WithDescription("Assign a support ticket to a specific admin");

        // group.MapPost("/support-tickets/{ticketId:int}/escalate", EscalateSupportTicketBySystemAdmin)
        //     .WithName("EscalateSupportTicketBySystemAdmin")
        //     .WithSummary("Escalate support ticket")
        //     .WithDescription("Escalate a support ticket to higher priority");

        // System Usage Monitoring
        group.MapGet("/system-usage", GetSystemUsageStatsBySystemAdmin)
            .WithName("GetSystemUsageStatsBySystemAdmin")
            .WithSummary("Get system usage statistics")
            .WithDescription("Retrieve comprehensive system usage statistics and metrics");

        group.MapGet("/dashboard", GetSystemDashboardBySystemAdmin)
            .WithName("GetSystemDashboardBySystemAdmin")
            .WithSummary("Get system dashboard")
            .WithDescription("Retrieve system dashboard with key metrics, alerts, and recent activities");

        // group.MapGet("/alerts", GetActiveAlertsBySystemAdmin)
        //     .WithName("GetActiveAlertsBySystemAdmin")
        //     .WithSummary("Get active alerts")
        //     .WithDescription("Retrieve all active system alerts");

        // group.MapPost("/alerts/{alertId:guid}/resolve", ResolveAlertBySystemAdmin)
        //     .WithName("ResolveAlertBySystemAdmin")
        //     .WithSummary("Resolve alert")
        //     .WithDescription("Resolve a system alert");

        // group.MapGet("/activities", GetRecentActivitiesBySystemAdmin)
        //     .WithName("GetRecentActivitiesBySystemAdmin")
        //     .WithSummary("Get recent activities")
        //     .WithDescription("Retrieve recent system activities and events");

        // System Analytics
        group.MapGet("/analytics", GetSystemAnalyticsBySystemAdmin)
            .WithName("GetSystemAnalyticsBySystemAdmin")
            .WithSummary("Get system analytics")
            .WithDescription("Retrieve system analytics for a specific date range");

        group.MapGet("/top-users", GetTopUsersBySystemAdmin)
            .WithName("GetTopUsersBySystemAdmin")
            .WithSummary("Get top users")
            .WithDescription("Retrieve top users by activity and usage");

        group.MapGet("/top-organizations", GetTopOrganizationsBySystemAdmin)
            .WithName("GetTopOrganizationsBySystemAdmin")
            .WithSummary("Get top organizations")
            .WithDescription("Retrieve top organizations by activity and usage");

        group.MapGet("/revenue-analytics", GetRevenueAnalyticsBySystemAdmin)
            .WithName("GetRevenueAnalyticsBySystemAdmin")
            .WithSummary("Get revenue analytics")
            .WithDescription("Retrieve revenue analytics for a specific date range");

        // System Maintenance
        // group.MapPost("/maintenance", PerformSystemMaintenanceBySystemAdmin)
        //     .WithName("PerformSystemMaintenanceBySystemAdmin")
        //     .WithSummary("Perform system maintenance")
        //     .WithDescription("Perform system maintenance operations (super admin only)");

        // group.MapPost("/cache/clear", ClearSystemCacheBySystemAdmin)
        //     .WithName("ClearSystemCacheBySystemAdmin")
        //     .WithSummary("Clear system cache")
        //     .WithDescription("Clear all system caches");

        // group.MapPost("/backup", BackupSystemDataBySystemAdmin)
        //     .WithName("BackupSystemDataBySystemAdmin")
        //     .WithSummary("Backup system data")
        //     .WithDescription("Create a backup of system data");

        // group.MapPost("/restore", RestoreSystemDataBySystemAdmin)
        //     .WithName("RestoreSystemDataBySystemAdmin")
        //     .WithSummary("Restore system data")
        //     .WithDescription("Restore system data from backup");

        // // System Configuration
        // group.MapGet("/configuration", GetSystemConfigurationBySystemAdmin)
        //     .WithName("GetSystemConfigurationBySystemAdmin")
        //     .WithSummary("Get system configuration")
        //     .WithDescription("Retrieve current system configuration");

        // group.MapPut("/configuration", UpdateSystemConfigurationBySystemAdmin)
        //     .WithName("UpdateSystemConfigurationBySystemAdmin")
        //     .WithSummary("Update system configuration")
        //     .WithDescription("Update system configuration (super admin only)");

        // group.MapPost("/configuration/reset", ResetSystemConfigurationBySystemAdmin)
        //     .WithName("ResetSystemConfigurationBySystemAdmin")
        //     .WithSummary("Reset system configuration")
        //     .WithDescription("Reset system configuration to defaults (super admin only)");

        // Export Approval Management
        group.MapGet("/exports/pending-approval", GetPendingApprovalExportsBySystemAdmin)
            .WithName("GetPendingApprovalExportsBySystemAdmin")
            .WithSummary("Get pending approval exports")
            .WithDescription("Retrieve all exports pending admin approval");

        group.MapGet("/exports", GetAllExportsBySystemAdmin)
            .WithName("GetAllExportsBySystemAdmin")
            .WithSummary("Get all exports with pagination and filter")
            .WithDescription("Retrieve all exports (pending, approved, rejected) with pagination and optional status filter");

        group.MapPost("/exports/{exportId:int}/approve", ApproveExportBySystemAdmin)
            .WithName("ApproveExportBySystemAdmin")
            .WithSummary("Approve export")
            .WithDescription("Approve an export for download");

        group.MapPost("/exports/{exportId:int}/reject", RejectExportBySystemAdmin)
            .WithName("RejectExportBySystemAdmin")
            .WithSummary("Reject export")
            .WithDescription("Reject an export with a reason");
    }

    // System User Management
    private static async Task<IResult> GetAllUsersBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetAllUsersAsync(page, pageSize, search, status, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetUserDetailsBySystemAdmin(
        Guid userId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetUserDetailsAsync(userId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateUserStatusBySystemAdmin(
        Guid userId,
        [FromBody] UpdateUserStatusRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.UpdateUserStatusAsync(request, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> DeleteUserBySystemAdmin(
        Guid userId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.DeleteUserAsync(userId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ImpersonateUserBySystemAdmin(
        Guid userId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.ImpersonateUserAsync(userId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Organization Management
    private static async Task<IResult> GetAllOrganizationsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetAllOrganizationsAsync(page, pageSize, search, status, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetOrganizationDetailsBySystemAdmin(
        Guid orgId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetOrganizationDetailsAsync(orgId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateOrganizationStatusBySystemAdmin(
        Guid orgId,
        [FromBody] UpdateOrganizationStatusRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.UpdateOrganizationStatusAsync(request, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> DeleteOrganizationBySystemAdmin(
        Guid orgId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.DeleteOrganizationAsync(orgId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> TransferOrganizationOwnershipBySystemAdmin(
        Guid orgId,
        [FromBody] TransferOwnershipRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.TransferOrganizationOwnershipAsync(orgId, request.NewOwnerId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Subscription Plan Management
    private static async Task<IResult> GetAllSubscriptionPlansBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetAllSubscriptionPlansAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetSubscriptionPlanDetailsBySystemAdmin(
        int planId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetSubscriptionPlanDetailsAsync(planId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> CreateSubscriptionPlanBySystemAdmin(
        [FromBody] CreateSubscriptionPlanRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.CreateSubscriptionPlanAsync(request, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateSubscriptionPlanBySystemAdmin(
        int planId,
        [FromBody] UpdateSubscriptionPlanRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.UpdateSubscriptionPlanAsync(request, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> DeleteSubscriptionPlanBySystemAdmin(
        int planId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.DeleteSubscriptionPlanAsync(planId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ActivateSubscriptionPlanBySystemAdmin(
        int planId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.ActivateSubscriptionPlanAsync(planId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> DeactivateSubscriptionPlanBySystemAdmin(
        int planId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.DeactivateSubscriptionPlanAsync(planId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Support Ticket Management
    private static async Task<IResult> GetAllSupportTicketsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        string? priority = null,
        string? category = null,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetAllSupportTicketsAsync(page, pageSize, status, priority, category, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetSupportTicketDetailsBySystemAdmin(
        int ticketId,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetSupportTicketDetailsAsync(ticketId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateSupportTicketBySystemAdmin(
        int ticketId,
        [FromBody] SystemAdminUpdateSupportTicketRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        request.TicketId = ticketId;
        var result = await systemAdminService.UpdateSupportTicketAsync(request, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> CloseSupportTicketBySystemAdmin(
        int ticketId,
        [FromBody] SystemAdminCloseTicketRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.CloseSupportTicketAsync(ticketId, request.Resolution, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> AssignSupportTicketBySystemAdmin(
        int ticketId,
        [FromBody] SystemAdminAssignTicketRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.AssignSupportTicketAsync(ticketId, request.AssignedToUserId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> EscalateSupportTicketBySystemAdmin(
        int ticketId,
        [FromBody] SystemAdminEscalateTicketRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.EscalateSupportTicketAsync(ticketId, request.Reason, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Usage Monitoring
    private static async Task<IResult> GetSystemUsageStatsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetSystemUsageStatsAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetSystemDashboardBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetFlattenedSystemDashboardAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetActiveAlertsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetActiveAlertsAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ResolveAlertBySystemAdmin(
        Guid alertId,
        [FromBody] ResolveAlertRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.ResolveAlertAsync(alertId, request.Resolution, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetRecentActivitiesBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetRecentActivitiesAsync(page, pageSize, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Analytics
    private static async Task<IResult> GetSystemAnalyticsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetSystemAnalyticsAsync(startDate, endDate, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetTopUsersBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int count = 10,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetTopUsersAsync(count, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetTopOrganizationsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        int count = 10,
        CancellationToken ct = default)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetTopOrganizationsAsync(count, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetRevenueAnalyticsBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetDailyRevenueAnalyticsAsync(startDate, endDate, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Maintenance
    private static async Task<IResult> PerformSystemMaintenanceBySystemAdmin(
        [FromBody] MaintenanceRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.PerformSystemMaintenanceAsync(request.MaintenanceType, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ClearSystemCacheBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.ClearSystemCacheAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> BackupSystemDataBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.BackupSystemDataAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> RestoreSystemDataBySystemAdmin(
        [FromBody] RestoreRequest request,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.RestoreSystemDataAsync(request.BackupId, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // System Configuration
    private static async Task<IResult> GetSystemConfigurationBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.GetSystemConfigurationAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateSystemConfigurationBySystemAdmin(
        [FromBody] Dictionary<string, object> configuration,
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.UpdateSystemConfigurationAsync(configuration, ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ResetSystemConfigurationBySystemAdmin(
        ISystemAdminService systemAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!await IsSuperAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await systemAdminService.ResetSystemConfigurationAsync(ct);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // Export Approval Management
    private static async Task<IResult> GetPendingApprovalExportsBySystemAdmin(
        [FromServices] IExportService exportService,
        ClaimsPrincipal user,
        ISystemAdminService systemAdminService,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var result = await exportService.GetPendingApprovalExportsAsync();
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetAllExportsBySystemAdmin(
        [FromServices] IExportService exportService,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] int? status,
        ClaimsPrincipal user,
        ISystemAdminService systemAdminService,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        // Set defaults
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        // Convert status int to enum if provided
        CusomMapOSM_Domain.Entities.Exports.Enums.ExportStatusEnum? statusEnum = null;
        if (status.HasValue && Enum.IsDefined(typeof(CusomMapOSM_Domain.Entities.Exports.Enums.ExportStatusEnum), status.Value))
        {
            statusEnum = (CusomMapOSM_Domain.Entities.Exports.Enums.ExportStatusEnum)status.Value;
        }

        var result = await exportService.GetAllExportsAsync(page, pageSize, statusEnum);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> ApproveExportBySystemAdmin(
        int exportId,
        [FromServices] IExportService exportService,
        ClaimsPrincipal user,
        ISystemAdminService systemAdminService,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var adminUserId))
            return Results.BadRequest("Invalid user ID");

        var result = await exportService.ApproveExportAsync(exportId, adminUserId);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> RejectExportBySystemAdmin(
        int exportId,
        [FromBody] RejectExportRequest request,
        [FromServices] IExportService exportService,
        ClaimsPrincipal user,
        ISystemAdminService systemAdminService,
        CancellationToken ct)
    {
        if (!await IsSystemAdmin(user, systemAdminService, ct))
            return Results.Forbid();

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var adminUserId))
            return Results.BadRequest("Invalid user ID");

        var result = await exportService.RejectExportAsync(exportId, adminUserId, request.Reason);
        return result.Match(
            some: data => Results.Ok(data),
            none: error => error.ToProblemDetailsResult()
        );
    }

    // Helper Methods
    private static async Task<bool> IsSystemAdmin(ClaimsPrincipal user, ISystemAdminService systemAdminService, CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null) return false;

        var result = await systemAdminService.IsUserSystemAdminAsync(userId.Value, ct);
        return result.HasValue && result.ValueOrDefault();
    }

    private static async Task<bool> IsSuperAdmin(ClaimsPrincipal user, ISystemAdminService systemAdminService, CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null) return false;

        var result = await systemAdminService.IsUserSuperAdminAsync(userId.Value, ct);
        return result.HasValue && result.ValueOrDefault();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}

// Additional DTOs for the endpoint
public record TransferOwnershipRequest
{
    public required Guid NewOwnerId { get; set; }
}

public record SystemAdminCloseTicketRequest
{
    public required string Resolution { get; set; }
}

public record SystemAdminAssignTicketRequest
{
    public required Guid AssignedToUserId { get; set; }
}

public record SystemAdminEscalateTicketRequest
{
    public required string Reason { get; set; }
}

public record RejectExportRequest
{
    public required string Reason { get; set; }
}

public record ResolveAlertRequest
{
    public required string Resolution { get; set; }
}

public record MaintenanceRequest
{
    public required string MaintenanceType { get; set; }
}

public record RestoreRequest
{
    public required string BackupId { get; set; }
}
