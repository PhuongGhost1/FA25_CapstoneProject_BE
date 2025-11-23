using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

public class MapCollaborationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{Routes.Prefix.Maps}/collaboration")
            .WithTags(Tags.Map)
            .WithDescription("Map collaboration endpoints for real-time user selection tracking")
            .RequireAuthorization();

        group.MapGet("/{mapId:guid}/active-users", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapSelectionService selectionService,
                CancellationToken ct) =>
            {
                var result = await selectionService.GetActiveUsers(mapId);
                return result.Match(
                    users => Results.Ok(new GetActiveUsersResponse
                    {
                        MapId = mapId,
                        ActiveUsers = users,
                        TotalCount = users.Count
                    }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetActiveMapUsers")
            .WithDescription("Get all active users and their selections on a map")
            .Produces<GetActiveUsersResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{mapId:guid}/my-selection", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapSelectionService selectionService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var result = await selectionService.GetUserSelection(mapId, userId.Value);
                return result.Match(
                    selection => Results.Ok(selection),
                    error => Results.NotFound(new { Message = "No selection found" })
                );
            })
            .WithName("GetMySelection")
            .WithDescription("Get current user's selection on a map")
            .Produces<MapSelectionResponse>(200)
            .Produces(404)
            .ProducesProblem(500);

        group.MapPost("/selection", async (
                [FromBody] UpdateSelectionRequest request,
                [FromServices] IMapSelectionService selectionService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var result = await selectionService.UpdateSelection(request.MapId, userId.Value, request);
                return result.Match(
                    selection => Results.Ok(selection),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateSelection")
            .WithDescription("Update current user's selection (REST fallback - prefer SignalR)")
            .Produces<MapSelectionResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        group.MapDelete("/{mapId:guid}/selection", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapSelectionService selectionService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var result = await selectionService.ClearSelection(mapId, userId.Value);
                return result.Match(
                    success => Results.NoContent(),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ClearSelection")
            .WithDescription("Clear current user's selection")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(500);
    }
}


public class GetActiveUsersResponse
{
    public Guid MapId { get; set; }
    public List<ActiveMapUserResponse> ActiveUsers { get; set; } = new();
    public int TotalCount { get; set; }
}