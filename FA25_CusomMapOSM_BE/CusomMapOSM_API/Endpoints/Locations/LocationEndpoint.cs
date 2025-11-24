using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Locations;

public class LocationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Location)
            .WithTags(Tags.Locations)
            .WithDescription(Tags.Locations)
            .RequireAuthorization();

        MapReadEndpoints(group);
        MapMutationEndpoints(group);
    }

    private static void MapReadEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.LocationEndpoints.GetMapLocations, async (
                [FromRoute] Guid mapId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetMapLocations(mapId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetMapLocations")
            .WithDescription("Retrieve all locations for a map");

        group.MapGet(Routes.LocationEndpoints.GetSegmentLocations, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetSegmentLocationsAsync(mapId, segmentId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
                .WithName("GetSegmentLocations")
            .WithDescription("Retrieve locations scoped to a segment");

        group.MapGet(Routes.LocationEndpoints.GetZoneLocations, async (
                [FromRoute] Guid zoneId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetZoneLocationsAsync(zoneId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetZoneLocations")
            .WithDescription("Retrieve locations in a specific zone");

        group.MapGet(Routes.LocationEndpoints.GetSegmentLocationsWithoutZone, async (
                [FromRoute] Guid segmentId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.GetLocationsWithoutZoneAsync(segmentId, ct);
                return result.Match<IResult>(
                    locations => Results.Ok(locations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetLocationsWithoutZone")
            .WithDescription("Retrieve locations not assigned to any zone");
    }

    private static void MapMutationEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(Routes.LocationEndpoints.CreateMapLocation, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateLocationRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId };
                    var result = await locationService.CreateLocationAsync(enriched, ct);
                return result.Match<IResult>(
                    location => Results.Created($"{Routes.Prefix.Location}/{mapId}", location),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateLocation")
            .WithDescription("Create a new location for the map");

        group.MapPost(Routes.LocationEndpoints.CreateSegmentLocation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] CreateLocationRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var enriched = request with { MapId = mapId, SegmentId = segmentId };
                var result = await locationService.CreateLocationAsync(enriched, ct);
                return result.Match<IResult>(
                    location => Results.Created($"{Routes.Prefix.Location}/{mapId}/segments/{segmentId}", location),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateSegmentLocation")
            .WithDescription("Create a location tied to a segment");

        group.MapPut(Routes.LocationEndpoints.UpdateLocation, async (
                [FromRoute] Guid locationId,
                [FromBody] UpdateLocationRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdateLocationAsync(locationId, request, ct);
                return result.Match<IResult>(
                    location => Results.Ok(location),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdatePoi")
            .WithDescription("Update a point of interest");

        group.MapDelete(Routes.LocationEndpoints.DeleteLocation, async (
                [FromRoute] Guid locationId,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.DeleteLocationAsync(locationId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteLocation")
            .WithDescription("Delete a location");

        group.MapPut(Routes.LocationEndpoints.UpdateLocationDisplayConfig, async (
                [FromRoute] Guid locationId,
                [FromBody] UpdateLocationDisplayConfigRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdateLocationDisplayConfigAsync(locationId, request, ct);
                return result.Match<IResult>(
                    location => Results.Ok(location),
                    err => err.ToProblemDetailsResult());
            })
                .WithName("UpdateLocationDisplayConfig")
            .WithDescription("Update display configuration of a location");

        group.MapPut(Routes.LocationEndpoints.UpdateLocationInteractionConfig, async (
                [FromRoute] Guid locationId,
                [FromBody] UpdateLocationInteractionConfigRequest request,
                [FromServices] ILocationService locationService,
                CancellationToken ct) =>
            {
                var result = await locationService.UpdateLocationInteractionConfigAsync(locationId, request, ct);
                return result.Match<IResult>(
                    location => Results.Ok(location),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateLocationInteractionConfig")
            .WithDescription("Update interaction configuration of a location");

        // Upload POI Audio
        group.MapPost(Routes.LocationEndpoints.UploadLocationAudio, async (
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
                    var storageUrl = await firebaseStorageService.UploadFileAsync(file.FileName, stream, "location-audio");
                    return Results.Ok(new { audioUrl = storageUrl });
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("UploadLocationAudio")
            .WithDescription("Upload an audio file for location")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500);
    }
}
