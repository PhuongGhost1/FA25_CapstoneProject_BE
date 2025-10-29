using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
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
        MapTimelineEndpoints(group);
        MapSegmentTransitionsEndpoints(group);
        MapStoryElementLayersEndpoints(group);
        MapStoryMapOperations(group);
    }

    private static void MapSegmentsEndpoints(RouteGroupBuilder group)
    {
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
                [FromBody] CreateSegmentZoneRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enriched = request with { SegmentId = segmentId };
                var result = await service.CreateSegmentZoneAsync(enriched, ct);
                return result.Match<IResult>(
                    zone => Results.Created($"{Routes.Prefix.StoryMap}/segments/{segmentId}/zones/{zone.ZoneId}", zone),
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
                [FromBody] UpdateSegmentZoneRequest request,
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
                [FromBody] UpsertSegmentLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateSegmentLayerAsync(segmentId, request, ct);
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
                [FromBody] UpsertSegmentLayerRequest request,
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

    private static void MapTimelineEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetTimeline, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetTimelineAsync(mapId, ct);
                return result.Match<IResult>(
                    timeline => Results.Ok(timeline),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetStoryMapTimeline")
            .WithDescription("Retrieve timeline steps for a map")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateTimelineStep, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateTimelineStepRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId };
                var result = await service.CreateTimelineStepAsync(enriched, ct);
                return result.Match<IResult>(
                    step => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/timeline/{step.TimelineStepId}", step),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateStoryMapTimelineStep")
            .WithDescription("Create a timeline step for a map")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateTimelineStep, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid stepId,
                [FromBody] UpdateTimelineStepRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateTimelineStepAsync(stepId, request, ct);
                return result.Match<IResult>(
                    step => Results.Ok(step),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateStoryMapTimelineStep")
            .WithDescription("Update a timeline step")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteTimelineStep, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid stepId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteTimelineStepAsync(stepId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteStoryMapTimelineStep")
            .WithDescription("Delete a timeline step")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapSegmentTransitionsEndpoints(RouteGroupBuilder group)
    {

        group.MapPost(Routes.StoryMapEndpoints.PreviewTransition, async (
                [FromRoute] Guid mapId,
                [FromBody] PreviewTransitionRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.PreviewTransitionAsync(request, ct);
                return result.Match<IResult>(
                    preview => Results.Ok(preview),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("PreviewStoryMapTransition")
            .WithDescription("Preview a transition between two segments")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapStoryMapOperations(RouteGroupBuilder group)
    {
        // Export story
        group.MapGet("/{mapId:guid}/export", async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.ExportAsync(mapId, ct);
                return result.Match<IResult>(
                    data => Results.Ok(data),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("ExportStoryMap")
            .WithDescription("Export story definition for a map")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Import story
        group.MapPost("/{mapId:guid}/import", async (
                [FromRoute] Guid mapId,
                [FromBody] ImportStoryRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var withMap = request with { MapId = mapId };
                var result = await service.ImportAsync(withMap, ct);
                return result.Match<IResult>(
                    ok => Results.Ok(new ImportStoryResponse { Imported = ok }),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("ImportStoryMap")
            .WithDescription("Import story definition for a map (overwrite/upsert)")
            .WithTags(Tags.StoryMaps)
            .Produces<ImportStoryResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static void MapStoryElementLayersEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetStoryElementLayers, async (
                [FromRoute] Guid elementId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetStoryElementLayersAsync(elementId, ct);
                return result.Match<IResult>(
                    layers => Results.Ok(layers),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetStoryElementLayers")
            .WithDescription("Retrieve layers for a story element")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost(Routes.StoryMapEndpoints.CreateStoryElementLayer, async (
                [FromBody] CreateStoryElementLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateStoryElementLayerAsync(request, ct);
                return result.Match<IResult>(
                    layer => Results.Created($"{Routes.Prefix.StoryMap}/story-elements/layers/{layer.StoryElementLayerId}", layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateStoryElementLayer")
            .WithDescription("Create a new story element layer")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPut(Routes.StoryMapEndpoints.UpdateStoryElementLayer, async (
                [FromRoute] Guid storyElementLayerId,
                [FromBody] UpdateStoryElementLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateStoryElementLayerAsync(storyElementLayerId, request, ct);
                return result.Match<IResult>(
                    layer => Results.Ok(layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateStoryElementLayer")
            .WithDescription("Update a story element layer")
            .WithTags(Tags.StoryMaps)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete(Routes.StoryMapEndpoints.DeleteStoryElementLayer, async (
                [FromRoute] Guid storyElementLayerId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteStoryElementLayerAsync(storyElementLayerId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteStoryElementLayer")
            .WithDescription("Delete a story element layer")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}
