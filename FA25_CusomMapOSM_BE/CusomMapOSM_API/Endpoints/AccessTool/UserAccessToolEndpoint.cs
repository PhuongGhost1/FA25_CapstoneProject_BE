using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.AccessTool;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.AccessTool;

public class UserAccessToolEndpoint : IEndpoint
{
    private const string API_PREFIX = "user-access-tool";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX);

        group.MapGet("get-all", async (
            ClaimsPrincipal user,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.GetUserAccessToolsAsync(userId, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("GetUserAccessTools")
        .WithDescription("Get all user access tools")
        .Produces<IReadOnlyList<UserAccessTool>>();

        group.MapGet("get-active", async (
            ClaimsPrincipal user,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.GetActiveUserAccessToolsAsync(userId, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("GetActiveUserAccessTools")
        .WithDescription("Get all active user access tools")
        .Produces<IReadOnlyList<UserAccessTool>>();

        group.MapPost("grant-access", async (
            ClaimsPrincipal user,
            [FromBody] GrantAccessToToolRequest request,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.GrantAccessToToolAsync(userId, request.AccessToolId, request.ExpiredAt, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("GrantAccessToTool")
        .WithDescription("Grant access to a tool")
        .Produces<UserAccessTool>();

        group.MapPost("revoke-access", async (
            ClaimsPrincipal user,
            [FromBody] RevokeAccessToToolRequest request,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.RevokeAccessToToolAsync(userId, request.AccessToolId, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("RevokeAccessToTool")
        .WithDescription("Revoke access to a tool")
        .Produces<bool>();

        group.MapPost("grant-multiple-access", async (
            ClaimsPrincipal user,
            [FromBody] GrantMultipleAccessToToolRequest request,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.GrantAccessToToolsAsync(userId, request.AccessToolIds, request.ExpiredAt, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("GrantMultipleAccessToTools")
        .WithDescription("Grant access to multiple tools")
        .Produces<bool>();

        group.MapPost("revoke-all-access", async (
            ClaimsPrincipal user,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.RevokeAllAccessToolsAsync(userId, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("RevokeAllAccessTools")
        .WithDescription("Revoke all access tools")
        .Produces<bool>();

        group.MapPost("update-access-tools-for-membership", async (
            ClaimsPrincipal user,
            [FromBody] UpdateAccessToolsForMembershipRequest request,
            [FromServices] IUserAccessToolService userAccessToolService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, request.PlanId, request.MembershipExpiryDate, ct);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("UpdateAccessToolsForMembership")
        .WithDescription("Update access tools for membership")
        .Produces<bool>();
    }
}