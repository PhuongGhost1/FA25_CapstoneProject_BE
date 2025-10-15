using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
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

    }
}
