using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CusomMapOSM_API.Endpoints.Animations;

public class AnimationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"/{Routes.ApiBase}/{Routes.Prefix.Animations}");

        group.MapGet(Routes.AnimationEndpoints.GetByLayer, async (Guid layerId, ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.GetAnimationsByLayerAsync(layerId, ct);
            return result.Match<IResult>(Ok => Results.Ok(Ok), Err => Results.NotFound(Err));
        });

        group.MapGet(Routes.AnimationEndpoints.GetActive, async (ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.GetActiveAnimationsAsync(ct);
            return result.Match<IResult>(Ok => Results.Ok(Ok), Err => Results.NotFound(Err));
        });

        group.MapGet(Routes.AnimationEndpoints.GetById, async (Guid animationId, ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.GetAnimationAsync(animationId, ct);
            return result.Match<IResult>(Ok => Results.Ok(Ok), Err => Results.NotFound(Err));
        });

        group.MapPost(Routes.AnimationEndpoints.Create, async (CreateLayerAnimationRequest req, ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.CreateAnimationAsync(req, ct);
            return result.Match<IResult>(Ok => Results.Created($"/animations/{Ok.LayerAnimationId}", Ok), Err => Results.BadRequest(Err));
        });

        group.MapPut(Routes.AnimationEndpoints.Update, async (Guid animationId, UpdateLayerAnimationRequest req, ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAnimationAsync(animationId, req, ct);
            return result.Match<IResult>(Ok => Results.Ok(Ok), Err => Results.NotFound(Err));
        });

        group.MapDelete(Routes.AnimationEndpoints.Delete, async (Guid animationId, ILayerAnimationService service, CancellationToken ct) =>
        {
            var result = await service.DeleteAnimationAsync(animationId, ct);
            return result.Match<IResult>(Ok => Results.Ok(new { success = Ok }), Err => Results.NotFound(Err));
        });
    }
}

