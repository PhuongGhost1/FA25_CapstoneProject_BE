using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.POIs;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.PointsOfInterest;

public class PoiEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.PointOfInterest)
            .WithTags(Tags.PointsOfInterest)
            .WithDescription(Tags.PointsOfInterest)
            .RequireAuthorization();

        MapReadEndpoints(group);
        MapMutationEndpoints(group);
    }

    private static void MapReadEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.PoiEndpoints.GetMapPois, async (
                [FromRoute] Guid mapId,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.GetMapPoisAsync(mapId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetMapPois")
            .WithDescription("Retrieve all points of interest for a map");

        group.MapGet(Routes.PoiEndpoints.GetSegmentPois, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.GetSegmentPoisAsync(mapId, segmentId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetSegmentPois")
            .WithDescription("Retrieve points of interest scoped to a segment");
    }

    private static void MapMutationEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(Routes.PoiEndpoints.CreateMapPoi, async (
                [FromRoute] Guid mapId,
                [FromBody] CreatePoiRequest request,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId };
                var result = await poiService.CreatePoiAsync(enriched, ct);
                return result.Match<IResult>(
                    poi => Results.Created($"{Routes.Prefix.PointOfInterest}/{mapId}/{poi.PoiId}", poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreatePoi")
            .WithDescription("Create a new point of interest for the map");

        group.MapPost(Routes.PoiEndpoints.CreateSegmentPoi, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] CreatePoiRequest request,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId, SegmentId = segmentId };
                var result = await poiService.CreatePoiAsync(enriched, ct);
                return result.Match<IResult>(
                    poi => Results.Created($"{Routes.Prefix.PointOfInterest}/{mapId}/segments/{segmentId}/{poi.PoiId}", poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateSegmentPoi")
            .WithDescription("Create a point of interest tied to a segment");

        group.MapPut(Routes.PoiEndpoints.UpdatePoi, async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiRequest request,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.UpdatePoiAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoi")
            .WithDescription("Update a point of interest");

        group.MapDelete(Routes.PoiEndpoints.DeletePoi, async (
                [FromRoute] Guid poiId,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.DeletePoiAsync(poiId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeletePoi")
            .WithDescription("Delete a point of interest");

        group.MapPut("/pois/{poiId}/display-config", async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiDisplayConfigRequest request,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.UpdatePoiDisplayConfigAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoiDisplayConfig")
            .WithDescription("Update display configuration of a POI");

        group.MapPut("/pois/{poiId}/interaction-config", async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiInteractionConfigRequest request,
                [FromServices] IPoiService poiService,
                CancellationToken ct) =>
            {
                var result = await poiService.UpdatePoiInteractionConfigAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoiInteractionConfig")
            .WithDescription("Update interaction configuration of a POI");
    }
}
