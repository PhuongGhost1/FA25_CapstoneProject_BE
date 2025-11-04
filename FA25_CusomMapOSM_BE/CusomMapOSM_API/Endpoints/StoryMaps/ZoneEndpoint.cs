using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class ZoneEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Zone Master Data Management")
            .RequireAuthorization();

        MapZoneEndpoints(group);
    }

    private static void MapZoneEndpoints(RouteGroupBuilder group)
    {
        // GET all zones
        group.MapGet(Routes.StoryMapEndpoints.GetZones, async (
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetZonesAsync(ct);
                return result.Match<IResult>(
                    zones => Results.Ok(zones),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetZones")
            .WithDescription("Retrieve all zones (master data)")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<ZoneSummaryDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // GET zone by ID
        group.MapGet(Routes.StoryMapEndpoints.GetZone, async (
                [FromRoute] Guid zoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetZoneAsync(zoneId, ct);
                return result.Match<IResult>(
                    zone => Results.Ok(zone),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetZone")
            .WithDescription("Retrieve a specific zone by ID")
            .WithTags(Tags.StoryMaps)
            .Produces<ZoneDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET zones by parent
        group.MapGet(Routes.StoryMapEndpoints.GetZonesByParent, async (
                [FromRoute] Guid parentZoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetZonesByParentAsync(parentZoneId, ct);
                return result.Match<IResult>(
                    zones => Results.Ok(zones),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetZonesByParent")
            .WithDescription("Retrieve zones that belong to a parent zone")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<ZoneSummaryDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Search zones
        group.MapGet(Routes.StoryMapEndpoints.SearchZones, async (
                [FromQuery] string? name,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var searchTerm = name ?? string.Empty;
                var result = await service.SearchZonesAsync(searchTerm, ct);
                return result.Match<IResult>(
                    zones => Results.Ok(zones),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchZones")
            .WithDescription("Search zones by name or type")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<ZoneSummaryDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // POST create zone
        group.MapPost(Routes.StoryMapEndpoints.CreateZone, async (
                [FromBody] CreateZoneRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateZoneAsync(request, ct);
                return result.Match<IResult>(
                    zone => Results.Created($"{Routes.Prefix.StoryMap}/zones/{zone.ZoneId}", zone),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateZone")
            .WithDescription("Create a new zone (master data)")
            .WithTags(Tags.StoryMaps)
            .Produces<ZoneDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create zone from OSM data
        group.MapPost("zones/from-osm", async (
                [FromBody] CreateZoneFromOsmRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateZoneFromOsmAsync(request, ct);
                return result.Match<IResult>(
                    zone => Results.Created($"{Routes.Prefix.StoryMap}/zones/{zone.ZoneId}", zone),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateZoneFromOsm")
            .WithDescription("Create a new zone from OpenStreetMap data")
            .WithTags(Tags.StoryMaps)
            .Produces<ZoneDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // PUT update zone
        group.MapPut(Routes.StoryMapEndpoints.UpdateZone, async (
                [FromRoute] Guid zoneId,
                [FromBody] UpdateZoneRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateZoneAsync(zoneId, request, ct);
                return result.Match<IResult>(
                    zone => Results.Ok(zone),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateZone")
            .WithDescription("Update an existing zone")
            .WithTags(Tags.StoryMaps)
            .Produces<ZoneDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE zone
        group.MapDelete(Routes.StoryMapEndpoints.DeleteZone, async (
                [FromRoute] Guid zoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteZoneAsync(zoneId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteZone")
            .WithDescription("Delete a zone (master data)")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST sync zones from OSM
        group.MapPost(Routes.StoryMapEndpoints.SyncZonesFromOSM, async (
                [FromServices] IStoryMapService service,
                [FromBody] SyncZonesFromOSMRequest request,
                CancellationToken ct) =>
            {
                var result = await service.SyncZonesFromOSMAsync(request, ct);
                return result.Match<IResult>(
                    count => Results.Ok(new { syncedCount = count }),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SyncZonesFromOSM")
            .WithDescription("Sync zones from OpenStreetMap data")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<ZoneDto>>(200)
            .ProducesProblem(500);
    }
}
