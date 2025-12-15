using System.Threading;
using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Layers;
using CusomMapOSM_Application.Models.DTOs.Features.Layers;
using CusomMapOSM_Domain.Entities.Layers;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Layers;

public class LayerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Layers)
            .WithTags("Layers")
            .WithDescription("Layer management endpoints");

        // GET /api/v1/layers/available - Get all available layers for current user
        group.MapGet(Routes.LayerEndpoints.GetAvailable, async (
                [FromServices] ILayerService layerService,
                CancellationToken ct) =>
            {
                var result = await layerService.GetAvailableLayersAsync(ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetAvailableLayers")
            .WithDescription("Get all layers available to the current user (owned + public)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(500);

        // GET /api/v1/layers/{layerId} - Get layer by ID
        group.MapGet(Routes.LayerEndpoints.GetById, async (
                [FromRoute] Guid layerId,
                [FromServices] ILayerService layerService,
                CancellationToken ct) =>
            {
                var result = await layerService.GetLayerByIdAsync(layerId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetLayerById")
            .WithDescription("Get layer details by ID")
            .RequireAuthorization()
            .Produces<LayerDetailDto>(200)
            .Produces(401)
            .Produces(404)
            .Produces(500);

        // GET /api/v1/layers/map/{mapId} - Get all layers for a specific map
        group.MapGet(Routes.LayerEndpoints.GetByMap, async (
                [FromRoute] Guid mapId,
                [FromServices] ILayerService layerService,
                CancellationToken ct) =>
            {
                var result = await layerService.GetLayersByMapAsync(mapId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetLayersByMap")
            .WithDescription("Get all layers for a specific map")
            .RequireAuthorization()
            .Produces<List<LayerDetailDto>>(200)
            .Produces(401)
            .Produces(500);
    }
}
