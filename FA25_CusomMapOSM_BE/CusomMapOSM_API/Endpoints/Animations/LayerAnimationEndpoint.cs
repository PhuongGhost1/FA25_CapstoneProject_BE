using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Animations;

public class LayerAnimationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Animations)
            .WithTags(Tags.Animations)
            .WithDescription("Layer Animations");

        group.MapGet("/layers/{layerId:guid}", async (
                [FromRoute] Guid layerId,
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.GetAnimationsByLayerAsync(layerId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetLayerAnimations")
            .WithDescription("Get animations by layer ID")
            .WithTags(Tags.Animations)
            .Produces<List<LayerAnimationDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapGet("/{animationId:guid}", async (
                [FromRoute] Guid animationId,
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.GetAnimationAsync(animationId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetAnimationById")
            .WithDescription("Get animation by ID")
            .WithTags(Tags.Animations)
            .Produces<LayerAnimationDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapGet("/active", async (
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.GetActiveAnimationsAsync();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetActiveAnimations")
            .WithDescription("Get all active animations")
            .WithTags(Tags.Animations)
            .Produces<List<LayerAnimationDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost("/", async (
                [FromForm] CreateLayerAnimationRequest request,
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.CreateAnimationAsync(request);
                return result.Match(
                    success => Results.CreatedAtRoute(
                        "GetAnimationById",
                        new { animationId = success.LayerAnimationId },
                        success
                    ),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateAnimation")
            .WithDescription("Create a new animation")
            .WithTags(Tags.Animations)
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<LayerAnimationDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{animationId:guid}", async (
                [FromRoute] Guid animationId,
                [FromBody] UpdateLayerAnimationRequest request,
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.UpdateAnimationAsync(animationId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateAnimation")
            .WithDescription("Update an animation")
            .WithTags(Tags.Animations)
            .Accepts<UpdateLayerAnimationRequest>("application/json")
            .Produces<LayerAnimationDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapDelete("/{animationId:guid}", async (
                [FromRoute] Guid animationId,
                [FromServices] ILayerAnimationService layerAnimationService) =>
            {
                var result = await layerAnimationService.DeleteAnimationAsync(animationId);
                return result.Match(
                    success => success ? Results.NoContent() : Results.NotFound($"Animation with ID {animationId} not found"),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteAnimation")
            .WithDescription("Delete an animation")
            .WithTags(Tags.Animations)
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}