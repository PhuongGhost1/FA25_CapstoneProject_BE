using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Usage;

public class UsageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/usage")
            .WithTags("Usage")
            .WithDescription("Usage tracking and quota management endpoints")
            .RequireAuthorization();

        // Get user usage
        group.MapGet("/user/{orgId:guid}", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromServices] IUsageService usageService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await usageService.GetUserUsageAsync(userId, orgId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetUserUsage")
            .WithDescription("Get user usage quotas and current usage")
            .Produces<UserUsageResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Check user quota
        group.MapPost("/user/{orgId:guid}/check-quota", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromBody] CheckQuotaRequest request,
                [FromServices] IUsageService usageService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await usageService.CheckUserQuotaAsync(userId, orgId, request.ResourceType, request.RequestedAmount, ct);
                return result.Match(
                    success => success.IsAllowed ? Results.Ok(success) : Results.BadRequest(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CheckUserQuota")
            .WithDescription("Check if user has sufficient quota for a resource")
            .Produces<CheckQuotaResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Consume user quota
        group.MapPost("/user/{orgId:guid}/consume", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromBody] CheckQuotaRequest request,
                [FromServices] IUsageService usageService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await usageService.ConsumeUserQuotaAsync(userId, orgId, request.ResourceType, request.RequestedAmount, ct);
                return result.Match(
                    success => Results.Ok(new { success = true, message = "Quota consumed successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ConsumeUserQuota")
            .WithDescription("Consume user quota for a resource")
            .Produces<object>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get organization usage
        group.MapGet("/organization/{orgId:guid}", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromServices] IUsageService usageService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await usageService.GetOrganizationUsageAsync(orgId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetOrganizationUsage2")
            .WithDescription("Get comprehensive organization usage information including quotas, members, and limits")
            .Produces<OrganizationUsageResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get organization maps with usage info
        group.MapGet("/organization/{orgId:guid}/maps", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromServices] IMapService mapService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                // Get maps by organization (this will handle authorization checks internally)
                var result = await mapService.GetOrganizationMaps(orgId);
                return result.Match(
                    success => Results.Ok(new
                    {
                        organizationId = orgId,
                        organizationName = success.OrganizationName,
                        totalMaps = success.Maps.Count,
                        totalViews = success.Maps.Sum(m => m.Views ?? 0),
                        maps = success.Maps.Select(m => new
                        {
                            id = m.Id,
                            name = m.Name,
                            description = m.Description,
                            views = m.Views ?? 0,
                            isPublic = m.IsPublic,
                            status = m.Status.ToString(),
                            isStoryMap = m.IsStoryMap,
                            createdAt = m.CreatedAt,
                            updatedAt = m.UpdatedAt,
                            ownerId = m.OwnerId,
                            ownerName = m.OwnerName,
                            workspaceName = m.WorkspaceName
                        })
                    }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetOrganizationMapsUsage")
            .WithDescription("Get all maps and their usage statistics for an organization")
            .Produces<object>(200)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Check organization quota for maps
        group.MapPost("/organization/{orgId:guid}/check-quota", async (
                ClaimsPrincipal user,
                [FromRoute] Guid orgId,
                [FromBody] CheckQuotaRequest request,
                [FromServices] IUsageService usageService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await usageService.CheckOrganizationQuotaAsync(orgId, request.ResourceType, request.RequestedAmount, ct);
                return result.Match(
                    success => success.IsAllowed ? Results.Ok(success) : Results.BadRequest(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CheckOrganizationQuota")
            .WithDescription("Check if organization has sufficient quota for a resource")
            .Produces<CheckQuotaResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get plan limits by plan ID
        group.MapGet("/plans/{planId:int}/limits", async (
                [FromRoute] int planId,
                [FromServices] IMembershipService membershipService,
                CancellationToken ct) =>
            {
                var result = await membershipService.GetPlanLimitsAsync(planId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetPlanLimits")
            .WithDescription("Get usage limits for a specific plan by plan ID")
            .Produces<PlanLimitsResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

    }
}
