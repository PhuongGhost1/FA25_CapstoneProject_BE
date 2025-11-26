using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class AnimatedLayerPresetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Animated Layer Preset Management (Reusable Templates)")
            .RequireAuthorization();

        MapAnimatedLayerPresetEndpoints(group);
    }

    private static void MapAnimatedLayerPresetEndpoints(RouteGroupBuilder group)
    {
        // GET all animated layer presets
        group.MapGet(Routes.StoryMapEndpoints.GetAnimatedLayerPresets, async (
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetAnimatedLayerPresetsAsync(ct);
                return result.Match<IResult>(
                    presets => Results.Ok(presets),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetAnimatedLayerPresets")
            .WithDescription("Retrieve all animated layer presets (reusable templates)")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<AnimatedLayerPresetDto>>(200)
            .ProducesProblem(500);

        // GET animated layer preset by ID
        group.MapGet(Routes.StoryMapEndpoints.GetAnimatedLayerPreset, async (
                [FromRoute] Guid presetId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetAnimatedLayerPresetAsync(presetId, ct);
                return result.Match<IResult>(
                    preset => Results.Ok(preset),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetAnimatedLayerPreset")
            .WithDescription("Retrieve a specific animated layer preset")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerPresetDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Search animated layer presets
        group.MapGet(Routes.StoryMapEndpoints.SearchAnimatedLayerPresets, async (
                [FromQuery] string? name,
                [FromQuery] string? category,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var searchTerm = name ?? category ?? string.Empty;
                var result = await service.SearchAnimatedLayerPresetsAsync(searchTerm, ct);
                return result.Match<IResult>(
                    presets => Results.Ok(presets),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("SearchAnimatedLayerPresets")
            .WithDescription("Search animated layer presets by name or category")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<AnimatedLayerPresetDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // POST create animated layer preset
        group.MapPost(Routes.StoryMapEndpoints.CreateAnimatedLayerPreset, async (
                [FromBody] CreateAnimatedLayerPresetRequest request,
                [FromServices] IStoryMapService service,
                [FromServices] CusomMapOSM_Application.Interfaces.Services.User.ICurrentUserService currentUserService,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                var result = await service.CreateAnimatedLayerPresetAsync(request, userId, ct);
                return result.Match<IResult>(
                    preset => Results.Created($"{Routes.Prefix.StoryMap}/animated-layer-presets/{preset.AnimatedLayerPresetId}", preset),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateAnimatedLayerPreset")
            .WithDescription("Create a new animated layer preset (reusable template)")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerPresetDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // PUT update animated layer preset
        group.MapPut(Routes.StoryMapEndpoints.UpdateAnimatedLayerPreset, async (
                [FromRoute] Guid presetId,
                [FromBody] UpdateAnimatedLayerPresetRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateAnimatedLayerPresetAsync(presetId, request, ct);
                return result.Match<IResult>(
                    preset => Results.Ok(preset),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateAnimatedLayerPreset")
            .WithDescription("Update an existing animated layer preset")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerPresetDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE animated layer preset
        group.MapDelete(Routes.StoryMapEndpoints.DeleteAnimatedLayerPreset, async (
                [FromRoute] Guid presetId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteAnimatedLayerPresetAsync(presetId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteAnimatedLayerPreset")
            .WithDescription("Delete an animated layer preset")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create animated layer from preset
        group.MapPost(Routes.StoryMapEndpoints.CreateAnimatedLayerFromPreset, async (
                [FromRoute] Guid presetId,
                [FromQuery] Guid? layerId,
                [FromQuery] Guid? segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.CreateAnimatedLayerFromPresetAsync(presetId, layerId, segmentId, ct);
                return result.Match<IResult>(
                    layer => Results.Created($"{Routes.Prefix.StoryMap}/animated-layers/{layer.AnimatedLayerId}", layer),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateAnimatedLayerFromPreset")
            .WithDescription("Create a new animated layer from a preset template")
            .WithTags(Tags.StoryMaps)
            .Produces<AnimatedLayerDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}
