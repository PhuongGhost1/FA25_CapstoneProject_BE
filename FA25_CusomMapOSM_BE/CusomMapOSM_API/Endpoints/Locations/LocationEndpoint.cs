using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Locations;

public class LocationEndpoint : IEndpoint
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
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetMapPoisAsync(mapId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetMapPois")
            .WithDescription("Retrieve all points of interest for a map");

        group.MapGet(Routes.PoiEndpoints.GetSegmentPois, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetSegmentPoisAsync(mapId, segmentId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetSegmentPois")
            .WithDescription("Retrieve points of interest scoped to a segment");

        group.MapGet("/zones/{zoneId}/pois", async (
                [FromRoute] Guid zoneId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetZonePoisAsync(zoneId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetZonePois")
            .WithDescription("Retrieve points of interest in a specific zone");

        group.MapGet("/segments/{segmentId}/pois/without-zone", async (
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetPoisWithoutZoneAsync(segmentId, ct);
                return result.Match<IResult>(
                    pois => Results.Ok(pois),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetPoisWithoutZone")
            .WithDescription("Retrieve points of interest not assigned to any zone");
    }

    private static void MapMutationEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(Routes.PoiEndpoints.CreateMapPoi, async (
                [FromRoute] Guid mapId,
                [FromBody] CreatePoiRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId };
                var result = await locationService.CreatePoiAsync(enriched, ct);
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
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId, SegmentId = segmentId };
                var result = await locationService.CreatePoiAsync(enriched, ct);
                return result.Match<IResult>(
                    poi => Results.Created($"{Routes.Prefix.PointOfInterest}/{mapId}/segments/{segmentId}/{poi.PoiId}", poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateSegmentPoi")
            .WithDescription("Create a point of interest tied to a segment");

        group.MapPut(Routes.PoiEndpoints.UpdatePoi, async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdatePoiAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoi")
            .WithDescription("Update a point of interest");

        group.MapDelete(Routes.PoiEndpoints.DeletePoi, async (
                [FromRoute] Guid poiId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.DeletePoiAsync(poiId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeletePoi")
            .WithDescription("Delete a point of interest");

        group.MapPut("/pois/{poiId}/display-config", async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiDisplayConfigRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdatePoiDisplayConfigAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoiDisplayConfig")
            .WithDescription("Update display configuration of a POI");

        group.MapPut("/pois/{poiId}/interaction-config", async (
                [FromRoute] Guid poiId,
                [FromBody] UpdatePoiInteractionConfigRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdatePoiInteractionConfigAsync(poiId, request, ct);
                return result.Match<IResult>(
                    poi => Results.Ok(poi),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoiInteractionConfig")
            .WithDescription("Update interaction configuration of a POI");

        // Upload POI Audio
        group.MapPost("/pois/upload-audio", async (
                IFormFile file,
                [FromServices] IFirebaseStorageService firebaseStorageService,
                CancellationToken ct) =>
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "No file provided" });
                }

                var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return Results.BadRequest(new { error = "Invalid file type. Only audio files are allowed." });
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    var storageUrl = await firebaseStorageService.UploadFileAsync(file.FileName, stream, "poi-audio");
                    return Results.Ok(new { audioUrl = storageUrl });
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("UploadPoiAudio")
            .WithDescription("Upload an audio file for POI")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500);
    }
}
