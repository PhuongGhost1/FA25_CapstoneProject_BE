using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
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

        // GET search locations
        group.MapGet(Routes.StoryMapEndpoints.SearchLocations, async (
                [FromQuery] string? name,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var searchTerm = name ?? string.Empty;
                var result = await service.SearchLocationsAsync(searchTerm, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchLocations")
            .WithDescription("Search locations by name (checks DB first, then OSM)")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<LocationDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // GET search routes
        group.MapGet(Routes.StoryMapEndpoints.SearchRoutes, async (
                [FromQuery] string? from,
                [FromQuery] string? to,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var fromLocation = from ?? string.Empty;
                var toLocation = to ?? string.Empty;
                var result = await service.SearchRoutesAsync(fromLocation, toLocation, ct);
                return result.Match<IResult>(
                    routes => Results.Ok(routes),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchRoutes")
            .WithDescription("Search routes between two locations (checks DB first, then OSM)")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<ZoneDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // GET search route between two location IDs
        group.MapGet(Routes.StoryMapEndpoints.SearchRouteBetweenLocations, async (
                [FromQuery] Guid fromLocationId,
                [FromQuery] Guid toLocationId,
                [FromQuery] string? routeType,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var routeTypeValue = routeType ?? "road"; // Default to road
                var result = await service.SearchRouteBetweenLocationsAsync(fromLocationId, toLocationId, routeTypeValue, ct);
                return result.Match<IResult>(
                    routeGeoJson => Results.Ok(new { routePath = routeGeoJson }),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchRouteBetweenLocations")
            .WithDescription("Search route between two location IDs (returns GeoJSON LineString). routeType: 'road' for actual road route, 'straight' or 'plane' for straight line")
            .WithTags(Tags.StoryMaps)
            .Produces<object>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET search route with multiple locations (waypoints)
        group.MapPost(Routes.StoryMapEndpoints.SearchRouteWithMultipleLocations, async (
                [FromBody] SearchRouteWithMultipleLocationsRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.SearchRouteWithMultipleLocationsAsync(request.LocationIds, request.RouteType ?? "road", ct);
                return result.Match<IResult>(
                    routeGeoJson => Results.Ok(new { routePath = routeGeoJson }),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchRouteWithMultipleLocations")
            .WithDescription("Search route through multiple location IDs (returns GeoJSON LineString). Supports locations from different segments.")
            .WithTags(Tags.StoryMaps)
            .Produces<object>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}

public record SearchRouteWithMultipleLocationsRequest
{
    public required List<Guid> LocationIds { get; init; }
    public string? RouteType { get; init; } // "road" or "straight"
}
