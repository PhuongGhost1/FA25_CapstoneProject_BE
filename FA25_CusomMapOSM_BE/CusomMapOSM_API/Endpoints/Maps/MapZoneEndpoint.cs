using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

/// <summary>
/// API endpoints for managing zones attached directly to maps (for non-StoryMap mode).
/// These endpoints allow adding, updating, and removing zones from maps without going through segments.
/// </summary>
public class MapZoneEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Maps)
            .WithTags("Maps")
            .WithDescription("Map Zone Management");

        MapMapZoneEndpoints(group);
    }

    private static void MapMapZoneEndpoints(RouteGroupBuilder group)
    {
        // GET all zones for a map
        group.MapGet(Routes.MapsEndpoints.GetMapZones, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                [FromServices] IMapService mapService,
                CancellationToken ct) =>
            {
                // Check if map exists
                var mapResult = await mapService.GetById(mapId);
                if (!mapResult.HasValue)
                {
                    return Results.NotFound(new { message = "Map not found" });
                }

                var result = await service.GetMapZonesAsync(mapId, ct);
                return result.Match<IResult>(
                    zones => Results.Ok(zones),
                    err => err.ToProblemDetailsResult());
            })
            .AllowAnonymous()
            .WithName("GetMapZones")
            .WithDescription("Retrieve all zones attached directly to a map (for non-StoryMap mode)")
            .WithTags("Maps")
            .Produces<IEnumerable<MapZoneDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create a new map zone
        group.MapPost(Routes.MapsEndpoints.CreateMapZone, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateMapZoneRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enrichedRequest = request with { MapId = mapId };
                var result = await service.CreateMapZoneAsync(enrichedRequest, ct);
                return result.Match<IResult>(
                    mapZone => Results.Created($"{Routes.Prefix.Maps}/{mapId}/zones/{mapZone.MapZoneId}", mapZone),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("CreateMapZone")
            .WithDescription("Create a new zone attachment for a map (for non-StoryMap mode)")
            .WithTags("Maps")
            .Produces<MapZoneDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // PUT update a map zone
        group.MapPut(Routes.MapsEndpoints.UpdateMapZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid mapZoneId,
                [FromBody] UpdateMapZoneRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateMapZoneAsync(mapZoneId, request, ct);
                return result.Match<IResult>(
                    mapZone => Results.Ok(mapZone),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("UpdateMapZone")
            .WithDescription("Update a map zone's display properties")
            .WithTags("Maps")
            .Produces<MapZoneDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE a map zone
        group.MapDelete(Routes.MapsEndpoints.DeleteMapZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid mapZoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteMapZoneAsync(mapZoneId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DeleteMapZone")
            .WithDescription("Remove a zone from a map")
            .WithTags("Maps")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}
