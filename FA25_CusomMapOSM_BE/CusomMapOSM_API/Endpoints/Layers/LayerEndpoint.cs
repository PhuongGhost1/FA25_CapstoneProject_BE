using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using CustomMapOSMDbContext = CusomMapOSM_Infrastructure.Databases.CustomMapOSMDbContext;

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
                CustomMapOSMDbContext dbContext,
                ICurrentUserService currentUserService) =>
            {
                try
                {
                    var userId = currentUserService.GetUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }

                    // Get layers owned by user + public layers
                    var layers = await dbContext.Layers
                        .Where(l => l.UserId == userId.Value || l.IsPublic)
                        .OrderByDescending(l => l.CreatedAt)
                        .Select(l => new
                        {
                            l.LayerId,
                            l.MapId,
                            l.LayerName,
                            l.LayerType,
                            l.SourceType,
                            l.IsPublic,
                            l.FeatureCount,
                            l.DataSizeKB,
                            l.DataBounds,
                            l.CreatedAt,
                            l.UpdatedAt
                        })
                        .ToListAsync();

                    return Results.Ok(layers);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Failed to get available layers",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("GetAvailableLayers")
            .WithDescription("Get all layers available to the current user (owned + public)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(500);

        // GET /api/v1/layers/{layerId} - Get layer by ID
        group.MapGet(Routes.LayerEndpoints.GetById, async (
                Guid layerId,
                CustomMapOSMDbContext dbContext,
                ICurrentUserService currentUserService) =>
            {
                try
                {
                    var userId = currentUserService.GetUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }

                    var layer = await dbContext.Layers
                        .Where(l => l.LayerId == layerId && (l.UserId == userId.Value || l.IsPublic))
                        .FirstOrDefaultAsync();

                    if (layer == null)
                    {
                        return Results.NotFound(new { message = "Layer not found or access denied" });
                    }

                    return Results.Ok(layer);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Failed to get layer",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("GetLayerById")
            .WithDescription("Get layer details by ID")
            .RequireAuthorization()
            .Produces<Layer>(200)
            .Produces(401)
            .Produces(404)
            .Produces(500);

        // GET /api/v1/layers/map/{mapId} - Get all layers for a specific map
        group.MapGet(Routes.LayerEndpoints.GetByMap, async (
                Guid mapId,
                CustomMapOSMDbContext dbContext,
                ICurrentUserService currentUserService) =>
            {
                try
                {
                    var userId = currentUserService.GetUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }

                    var layers = await dbContext.Layers
                        .Where(l => l.MapId == mapId && (l.UserId == userId.Value || l.IsPublic))
                        .OrderByDescending(l => l.CreatedAt)
                        .ToListAsync();

                    return Results.Ok(layers);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Failed to get map layers",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("GetLayersByMap")
            .WithDescription("Get all layers for a specific map")
            .RequireAuthorization()
            .Produces<List<Layer>>(200)
            .Produces(401)
            .Produces(500);
    }
}
