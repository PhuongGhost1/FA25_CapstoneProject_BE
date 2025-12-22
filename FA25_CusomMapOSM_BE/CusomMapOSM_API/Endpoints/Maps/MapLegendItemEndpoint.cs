using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

public class MapLegendItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Maps + "/{mapId:guid}/legend-items")
            .WithTags(Tags.Map)
            .WithDescription("Map legend item management endpoints");

        // Get all legend items for a map
        group.MapGet("/", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapLegendItemService legendItemService,
                CancellationToken ct) =>
            {
                var result = await legendItemService.GetByMapId(mapId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapLegendItems")
            .WithDescription("Get all legend items for a map")
            .Produces<GetMapLegendItemsResponse>(200)
            .ProducesProblem(404);

        // Get a single legend item
        group.MapGet("/{legendItemId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid legendItemId,
                [FromServices] IMapLegendItemService legendItemService,
                CancellationToken ct) =>
            {
                var result = await legendItemService.GetById(mapId, legendItemId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapLegendItemById")
            .WithDescription("Get a single legend item by ID")
            .Produces<MapLegendItemDto>(200)
            .ProducesProblem(404);

        // Create a new legend item
        group.MapPost("/", async (
                [FromRoute] Guid mapId,
                [FromBody] CreateMapLegendItemRequest request,
                [FromServices] IMapLegendItemService legendItemService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await legendItemService.Create(mapId, userId.Value, request, ct);
                return result.Match(
                    success => Results.Created($"/api/maps/{mapId}/legend-items/{success.LegendItemId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateMapLegendItem")
            .WithDescription("Create a new legend item for a map")
            .RequireAuthorization()
            .Produces<CreateMapLegendItemResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404);

        // Update a legend item
        group.MapPut("/{legendItemId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid legendItemId,
                [FromBody] UpdateMapLegendItemRequest request,
                [FromServices] IMapLegendItemService legendItemService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await legendItemService.Update(mapId, legendItemId, userId.Value, request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateMapLegendItem")
            .WithDescription("Update a legend item")
            .RequireAuthorization()
            .Produces<UpdateMapLegendItemResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404);

        // Delete a legend item
        group.MapDelete("/{legendItemId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid legendItemId,
                [FromServices] IMapLegendItemService legendItemService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await legendItemService.Delete(mapId, legendItemId, userId.Value, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteMapLegendItem")
            .WithDescription("Delete a legend item")
            .RequireAuthorization()
            .Produces<DeleteMapLegendItemResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(404);

        // Reorder legend items
        group.MapPut("/reorder", async (
                [FromRoute] Guid mapId,
                [FromBody] ReorderMapLegendItemsRequest request,
                [FromServices] IMapLegendItemService legendItemService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await legendItemService.Reorder(mapId, userId.Value, request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ReorderMapLegendItems")
            .WithDescription("Reorder legend items for a map")
            .RequireAuthorization()
            .Produces<ReorderMapLegendItemsResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404);
    }
}
