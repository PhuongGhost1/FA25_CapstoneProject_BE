using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class AnimatedLayerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Animated Layer Management (GIF/Video Overlays)")
            .RequireAuthorization();

        MapAnimatedLayerEndpoints(group);
    }

    private static void MapAnimatedLayerEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(Routes.StoryMapEndpoints.GetAnimatedLayers, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetAnimatedLayersAsync(mapId, ct);
                return result.Match<IResult>(
                    layers => Results.Ok(layers),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetAnimatedLayers")
            .WithDescription("Retrieve all animated layers for a map")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<AnimatedLayerDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET animated layer by ID
        group.MapGet(Routes.StoryMapEndpoints.GetAnimatedLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetAnimatedLayerAsync(layerId, ct);
                return result.Match<IResult>(
                    layer => Results.Ok(layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetAnimatedLayer")
            .WithDescription("Retrieve a specific animated layer")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create animated layer
        group.MapPost(Routes.StoryMapEndpoints.CreateAnimatedLayer, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateAnimatedLayerRequest request,
                [FromServices] IStoryMapService service,
                [FromServices] ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (!userId.HasValue)
                {
                    return Results.Unauthorized();
                }
                var result = await service.CreateAnimatedLayerAsync(request, userId.Value, ct);
                return result.Match<IResult>(
                    layer => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/animated-layers/{layer.AnimatedLayerId}", layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateAnimatedLayer")
            .WithDescription("Create a new animated layer (GIF/Video overlay)")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // PUT update animated layer
        group.MapPut(Routes.StoryMapEndpoints.UpdateAnimatedLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromBody] UpdateAnimatedLayerRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateAnimatedLayerAsync(layerId, request, ct);
                return result.Match<IResult>(
                    layer => Results.Ok(layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateAnimatedLayer")
            .WithDescription("Update an existing animated layer")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE animated layer
        group.MapDelete(Routes.StoryMapEndpoints.DeleteAnimatedLayer, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteAnimatedLayerAsync(layerId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteAnimatedLayer")
            .WithDescription("Delete an animated layer")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST attach animated layer to segment - removed because AttachAnimatedLayerToSegmentAsync doesn't exist in interface
        // TODO: Implement AttachAnimatedLayerToSegmentAsync in IStoryMapService if needed
        // Or use UpdateAnimatedLayerAsync to change SegmentId
    }
}
