using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class StoryMapEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription(Tags.StoryMaps);

        MapSegmentsEndpoints(group);
        MapSegmentZonesEndpoints(group);
        MapSegmentLayersEndpoints(group);
        MapSegmentLocationsEndpoints(group);
    }

    private static void MapSegmentsEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetSegments, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService storyMapService,
                [FromServices] IMapService mapService,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                // Check if map exists
                var mapResult = await mapService.GetById(mapId);
                if (!mapResult.HasValue)
                {
                    return Results.NotFound(new { message = "Map not found" });
                }
                var result = await storyMapService.GetSegmentsAsync(mapId, ct);
                return result.Match<IResult>(
                    segments => Results.Ok(segments),
                    err => err.ToProblemDetailsResult());
            })
            .AllowAnonymous() 
            .WithName("GetStoryMapSegments")
            .WithDescription("Retrieve all story segments for a map. Public access if map is published.")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<SegmentDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapGet(Routes.StoryMapEndpoints.GetSegmentEnhanced, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentAsync(segmentId, ct);
                return result.Match<IResult>(
                    segment => Results.Ok(segment),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("GetStoryMapSegmentEnhanced")
            .WithDescription("Retrieve a single segment with all details (zones, layers, transitions)")
            .WithTags(Tags.StoryMaps)
            .Produces<SegmentDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateSegment, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateSegmentRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enrichedRequest = request with { MapId = mapId };
                var result = await service.CreateSegmentAsync(enrichedRequest, ct);
                return result.Match<IResult>(
                    segment => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/segments/{segment.SegmentId}", segment),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("CreateStoryMapSegment")
            .WithDescription("Create a new story segment for the map")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] UpdateSegmentRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateSegmentAsync(segmentId, request, ct);
                return result.Match<IResult>(
                    segment => Results.Ok(segment),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("UpdateStoryMapSegment")
            .WithDescription("Update an existing story segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteSegmentAsync(segmentId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DeleteStoryMapSegment")
            .WithDescription("Delete a story segment")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.DuplicateSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DuplicateSegmentAsync(segmentId, ct);
                return result.Match<IResult>(
                    newSegment => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/segments/{newSegment.SegmentId}", newSegment),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DuplicateStoryMapSegment")
            .WithDescription("Duplicate a segment with all its zones and layers (deep copy)")
            .WithTags(Tags.StoryMaps)
            .Produces<SegmentDto>(201)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST reorder segments
        group.MapPost(Routes.StoryMapEndpoints.ReorderSegments, async (
                [FromRoute] Guid mapId,
                [FromBody] List<Guid> segmentIds,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.ReorderSegmentsAsync(mapId, segmentIds, ct);
                return result.Match<IResult>(
                    segments => Results.Ok(segments),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("ReorderStoryMapSegments")
            .WithDescription("Reorder segments in the timeline")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<SegmentDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapSegmentZonesEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetSegmentZones, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentZonesAsync(segmentId, ct);
                return result.Match<IResult>(
                    zones => Results.Ok(zones),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("GetStoryMapSegmentZones")
            .WithDescription("Retrieve zones for a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateSegmentZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] CreateSegmentZoneV2Request request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enriched = request with { SegmentId = segmentId };
                var result = await service.CreateSegmentZoneAsync(enriched, ct);
                return result.Match<IResult>(
                    zone => Results.Created($"{Routes.Prefix.StoryMap}/segments/{segmentId}/zones/{zone.SegmentZoneId}", zone),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("CreateStoryMapSegmentZone")
            .WithDescription("Create a new zone within a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateSegmentZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid segmentZoneId,
                [FromBody] UpdateSegmentZoneV2Request request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateSegmentZoneAsync(segmentZoneId, request, ct);
                return result.Match<IResult>(
                    zone => Results.Ok(zone),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("UpdateStoryMapSegmentZone")
            .WithDescription("Update a segment zone (relationship properties)")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteSegmentZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid segmentZoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteSegmentZoneAsync(segmentZoneId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DeleteStoryMapSegmentZone")
            .WithDescription("Delete a segment zone (removes zone from segment)")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.MoveZoneToSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid fromSegmentId,
                [FromRoute] Guid segmentZoneId,
                [FromRoute] Guid toSegmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.MoveZoneToSegmentAsync(segmentZoneId, fromSegmentId, toSegmentId, ct);
                return result.Match<IResult>(
                    success => Results.Ok(new { success = true, message = "Zone moved successfully" }),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("MoveZoneToSegment")
            .WithDescription("Move a zone from one segment to another")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapSegmentLayersEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetSegmentLayers, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentLayersAsync(segmentId, ct);
                return result.Match<IResult>(
                    layers => Results.Ok(layers),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("GetStoryMapSegmentLayers")
            .WithDescription("Retrieve layers attached to a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateSegmentLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] CreateSegmentLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enriched = request with { SegmentId = segmentId };
                var result = await service.CreateSegmentLayerAsync(enriched, ct);
                return result.Match<IResult>(
                    layer => Results.Created($"{Routes.Prefix.StoryMap}/segments/{segmentId}/layers/{layer.SegmentLayerId}", layer),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("CreateStoryMapSegmentLayer")
            .WithDescription("Attach a layer to a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateSegmentLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid layerId,
                [FromBody] UpdateSegmentLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateSegmentLayerAsync(layerId, request, ct);
                return result.Match<IResult>(
                    layer => Results.Ok(layer),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("UpdateStoryMapSegmentLayer")
            .WithDescription("Update a segment layer configuration")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteSegmentLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid layerId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteSegmentLayerAsync(layerId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DeleteStoryMapSegmentLayer")
            .WithDescription("Remove a layer from a segment")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.MoveLayerToSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid fromSegmentId,
                [FromRoute] Guid segmentLayerId,
                [FromRoute] Guid toSegmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.MoveLayerToSegmentAsync(segmentLayerId, fromSegmentId, toSegmentId, ct);
                return result.Match<IResult>(
                    success => Results.Ok(new { success = true, message = "Layer moved successfully" }),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("MoveLayerToSegment")
            .WithDescription("Move a layer from one segment to another")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapSegmentLocationsEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetSegmentLocations, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentLocationsAsync(mapId, segmentId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("GetStoryMapSegmentLocations")
            .WithDescription("Retrieve locations (POIs) for a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapGet(Routes.StoryMapEndpoints.GetMapLocations, async (
                [FromRoute] Guid mapId,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                var result = await service.GetMapLocations(mapId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .AllowAnonymous()
            .WithName("GetStoryMapLocations")
            .WithDescription("Retrieve all locations (POIs) for a map across all segments")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateSegmentLocation, async (
                [FromForm] CreateLocationRequest request,
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateLocationAsync(request, ct);
                return result.Match<IResult>(
                    location => Results.Created($"/api/v1/storymaps/{mapId}/segments/{segmentId}/locations/{location.LocationId}", location),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("CreateStoryMapSegmentLocation")
            .WithDescription("Create a new location (POI) for a segment with optional icon upload")
            .WithTags(Tags.StoryMaps)
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateSegmentLocation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid locationId,
                [FromBody] UpdateLocationRequest request,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                // Override segmentId from route
                var requestWithRoute = request with { SegmentId = segmentId };
                var result = await service.UpdateLocationAsync(locationId, requestWithRoute, ct);
                return result.Match<IResult>(
                    location => Results.Ok(location),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("UpdateStoryMapSegmentLocation")
            .WithDescription("Update a location (POI) in a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteSegmentLocation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid locationId,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteLocationAsync(locationId, ct);
                return result.Match<IResult>(
                    success => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("DeleteStoryMapSegmentLocation")
            .WithDescription("Delete a location (POI) from a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.MoveLocationToSegment, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid fromSegmentId,
                [FromRoute] Guid locationId,
                [FromRoute] Guid toSegmentId,
                [FromServices] ILocationService service,
                CancellationToken ct) =>
            {
                var result = await service.MoveLocationToSegmentAsync(locationId, fromSegmentId, toSegmentId, ct);
                return result.Match<IResult>(
                    success => Results.Ok(new { success = true, message = "Location moved successfully" }),
                    err => err.ToProblemDetailsResult());
            })
            .RequireAuthorization()
            .WithName("MoveLocationToSegment")
            .WithDescription("Move a location from one segment to another")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}
