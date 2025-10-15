using CusomMapOSM_Application.Interfaces.Features.OrganizationAdmin;
using CusomMapOSM_Application.Models.DTOs.Features.OrganizationAdmin;
using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_API.Extensions;
using static CusomMapOSM_API.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Optional.Unsafe;

namespace CusomMapOSM_API.Endpoints.OrgAdmin;

[Tags(OrganizationAdmin)]
public class OrganizationAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organization-admin")
            .WithTags(OrganizationAdmin)
            .RequireAuthorization();

        // Organization Management Endpoints (Org Owners/Admins)
        group.MapGet("/usage/{orgId:guid}", GetOrganizationUsage)
            .WithName("GetOrganizationUsage")
            .WithSummary("Monitor org-wide usage")
            .WithDescription("Retrieve comprehensive usage statistics for an organization including quotas, user summaries, and aggregated data");

        group.MapGet("/subscription/{orgId:guid}", GetOrganizationSubscription)
            .WithName("GetOrganizationSubscription")
            .WithSummary("View org subscription details")
            .WithDescription("Retrieve detailed subscription information including active, pending, and expired memberships");

        group.MapGet("/billing/{orgId:guid}", GetOrganizationBilling)
            .WithName("GetOrganizationBilling")
            .WithSummary("View billing information")
            .WithDescription("Retrieve billing information including recent transactions, invoices, and spending summaries");

        // Organization Usage Management
        group.MapPost("/usage/{orgId:guid}/check-quota", CheckOrganizationQuota)
            .WithName("CheckOrganizationQuotaByOrgAdmin")
            .WithSummary("Check organization quota")
            .WithDescription("Check if organization has sufficient quota for a resource");
    }

    private static async Task<IResult> GetOrganizationUsage(
        Guid orgId,
        IOrganizationAdminService organizationAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        // Check if user is admin or owner of the organization
        var isAdminResult = await organizationAdminService.IsUserOrganizationAdminAsync(userId.Value, orgId, ct);
        if (!isAdminResult.HasValue || !isAdminResult.ValueOrDefault())
            return Results.Forbid();

        var result = await organizationAdminService.GetOrganizationUsageAsync(orgId, ct);
        return result.HasValue ? Results.Ok(result.ValueOrDefault()) : Results.BadRequest("Failed to get organization usage");
    }


    private static async Task<IResult> GetOrganizationSubscription(
        Guid orgId,
        IOrganizationAdminService organizationAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var isAdminResult = await organizationAdminService.IsUserOrganizationAdminAsync(userId.Value, orgId, ct);
        if (!isAdminResult.HasValue || !isAdminResult.ValueOrDefault())
            return Results.Forbid();

        var result = await organizationAdminService.GetOrganizationSubscriptionAsync(orgId, ct);
        return result.HasValue ? Results.Ok(result.ValueOrDefault()) : Results.BadRequest("Failed to get organization subscription");
    }


    private static async Task<IResult> GetOrganizationBilling(
        Guid orgId,
        IOrganizationAdminService organizationAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var isAdminResult = await organizationAdminService.IsUserOrganizationAdminAsync(userId.Value, orgId, ct);
        if (!isAdminResult.HasValue || !isAdminResult.ValueOrDefault())
            return Results.Forbid();

        var result = await organizationAdminService.GetOrganizationBillingAsync(orgId, ct);
        return result.HasValue ? Results.Ok(result.ValueOrDefault()) : Results.BadRequest("Failed to get organization billing");
    }


    private static async Task<IResult> CheckOrganizationQuota(
        Guid orgId,
        [FromBody] CheckQuotaRequest request,
        IUsageService usageService,
        IOrganizationAdminService organizationAdminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        // Check if user is admin or owner of the organization
        var isAdminResult = await organizationAdminService.IsUserOrganizationAdminAsync(userId.Value, orgId, ct);
        if (!isAdminResult.HasValue || !isAdminResult.ValueOrDefault())
            return Results.Forbid();

        var result = await usageService.CheckOrganizationQuotaAsync(orgId, request.ResourceType, request.RequestedAmount, ct);
        return result.Match(
            success => success.IsAllowed ? Results.Ok(success) : Results.BadRequest(success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}

