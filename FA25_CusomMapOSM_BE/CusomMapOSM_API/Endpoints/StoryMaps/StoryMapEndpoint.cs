using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Features.POIs;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class StoryMapEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription(Tags.StoryMaps)
            .RequireAuthorization();

        MapSegmentsEndpoints(group);
        MapSegmentZonesEndpoints(group);
        MapSegmentLayersEndpoints(group);
        MapSegmentLocationsEndpoints(group);
    }

    private static void MapSegmentsEndpoints(RouteGroupBuilder group)
    {
        // GET all segments
        group.MapGet(Routes.StoryMapEndpoints.GetSegments, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentsAsync(mapId, ct);
                return result.Match<IResult>(
                    segments => Results.Ok(segments),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetStoryMapSegments")
            .WithDescription("Retrieve all story segments for a map")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET single segment with enhanced details
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
            .WithName("DeleteStoryMapSegment")
            .WithDescription("Delete a story segment")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST duplicate segment
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
            .WithName("CreateStoryMapSegmentZone")
            .WithDescription("Create a new zone within a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateSegmentZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid zoneId,
                [FromBody] UpdateSegmentZoneV2Request request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateSegmentZoneAsync(zoneId, request, ct);
                return result.Match<IResult>(
                    zone => Results.Ok(zone),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateStoryMapSegmentZone")
            .WithDescription("Update a segment zone")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteSegmentZone, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid zoneId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteSegmentZoneAsync(zoneId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteStoryMapSegmentZone")
            .WithDescription("Delete a segment zone")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
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
            .WithName("DeleteStoryMapSegmentLayer")
            .WithDescription("Remove a layer from a segment")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapSegmentLocationsEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetSegmentLocations, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IPoiService service,
                CancellationToken ct) =>
            {
                var result = await service.GetSegmentPoisAsync(mapId, segmentId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetStoryMapSegmentLocations")
            .WithDescription("Retrieve locations (POIs) for a segment")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}
