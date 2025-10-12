using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

public class MapEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/maps")
            .WithTags("Maps")
            .WithDescription("Map management endpoints");

        group.MapPost("/", async (
                [FromBody] CreateMapRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.Create(req);
                return result.Match(
                    success => Results.Created($"/api/maps/{success.MapId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateMap")
            .WithDescription("Create a new map")
            .RequireAuthorization()
            .Produces<CreateMapResponse>(201);

        group.MapPost("/from-template", async (
                [FromBody] CreateMapFromTemplateRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.CreateFromTemplate(req);
                return result.Match(
                    success => Results.Created($"/api/maps/{success.MapId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateMapFromTemplate")
            .WithDescription("Create a new map from template")
            .RequireAuthorization()
            .Produces<CreateMapFromTemplateResponse>(201);

        group.MapGet("/my", async (
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetMyMaps();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyMaps")
            .WithDescription("Get all maps owned by the current user")
            .RequireAuthorization()
            .Produces<GetMyMapsResponse>(200);

        group.MapGet("/organization/{orgId:guid}", async (
                [FromRoute] Guid orgId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetOrganizationMaps(orgId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetOrganizationMaps")
            .WithDescription("Get all maps for an organization")
            .RequireAuthorization()
            .Produces<GetOrganizationMapsResponse>(200);

        // Template endpoints
        group.MapGet("/templates", async (
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetTemplates();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapTemplates")
            .WithDescription("Get all available map templates")
            .AllowAnonymous()
            .Produces<GetMapTemplatesResponse>(200);

        group.MapGet("/templates/{templateId:guid}", async (
                [FromRoute] Guid templateId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetTemplateById(templateId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapTemplateById")
            .WithDescription("Get map template by ID")
            .AllowAnonymous()
            .Produces<GetMapTemplateByIdResponse>(200);

        group.MapGet("/templates/{templateId:guid}/details", async (
                Guid templateId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetTemplateWithDetails(templateId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapTemplateWithDetails")
            .WithDescription("Get map template with all details (layers, annotations, images)")
            .AllowAnonymous()
            .Produces<GetMapTemplateWithDetailsResponse>(200);

        group.MapGet("/templates/{templateId:guid}/layers/{layerId:guid}/data", async (
                [FromRoute] Guid templateId,
                [FromRoute] Guid layerId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetLayerData(templateId, layerId);
                return result.Match(
                    success => Results.Ok(new { layerData = success }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapTemplateLayerData")
            .WithDescription("Get layer GeoJSON data for map template")
            .AllowAnonymous()
            .Produces<object>(200)
            .Produces(404);

        group.MapGet("/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetById(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapById")
            .WithDescription("Get a specific map by ID")
            .RequireAuthorization()
            .Produces<GetMapByIdResponse>(200);

        group.MapPut("/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromBody] UpdateMapRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.Update(mapId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateMap")
            .WithDescription("Update a map")
            .RequireAuthorization()
            .Produces<UpdateMapResponse>(200);

        group.MapDelete("/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.Delete(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteMap")
            .WithDescription("Delete a map")
            .RequireAuthorization()
            .Produces<DeleteMapResponse>(200);

        group.MapPost("/{mapId:guid}/layers", async (
                [FromRoute] Guid mapId,
                [FromBody] AddLayerToMapRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.AddLayerToMap(mapId, req);
                return result.Match(
                    success => Results.Created($"/api/maps/{mapId}/layers/{success.MapLayerId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("AddLayerToMap")
            .WithDescription("Add a layer to a map")
            .RequireAuthorization()
            .Produces<AddLayerToMapResponse>(201);

        group.MapDelete("/{mapId:guid}/layers/{layerId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.RemoveLayerFromMap(mapId, layerId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("RemoveLayerFromMap")
            .WithDescription("Remove a layer from a map")
            .RequireAuthorization()
            .Produces<RemoveLayerFromMapResponse>(200);

        group.MapPatch("/{mapId:guid}/layers/{layerId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromBody] UpdateMapLayerRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.UpdateMapLayer(mapId, layerId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateMapLayer")
            .WithDescription("Update a map layer")
            .RequireAuthorization()
            .Produces<UpdateMapLayerResponse>(200);

        group.MapGet("/{mapId:guid}/layers", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetMapLayers(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapLayers")
            .WithDescription("Get all layers for a map")
            .RequireAuthorization()
            .Produces<List<LayerInfoResponse>>(200);

        group.MapPost("/{mapId:guid}/share", async (
                [FromRoute] Guid mapId,
                [FromBody] ShareMapRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.ShareMap(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ShareMap")
            .WithDescription("Share a map with another user")
            .RequireAuthorization()
            .Produces<ShareMapResponse>(200);

        group.MapDelete("/{mapId:guid}/share", async (
                [FromRoute] Guid mapId,
                [FromBody] UnshareMapRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.UnshareMap(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UnshareMap")
            .WithDescription("Remove sharing access for a map")
            .RequireAuthorization()
            .Produces<UnshareMapResponse>(200);

        group.MapPost("/template", CreateTemplateHandler)
            .WithName("CreateMapTemplateFromGeoJson")
            .WithDescription("Create MapTemplate from uploaded GeoJSON file")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .DisableAntiforgery();

        // Map Features endpoints
        group.MapPost("/{mapId:guid}/features", async (
                [FromRoute] Guid mapId,
                [FromBody] CreateMapFeatureRequest req,
                [FromServices] IMapFeatureService featureService) =>
            {
                req.MapId = mapId;
                var result = await featureService.Create(req);
                return result.Match(
                    success => Results.Created($"/api/maps/{mapId}/features/{success.FeatureId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateMapFeature")
            .WithDescription("Create a new feature on a map")
            .RequireAuthorization()
            .Produces<MapFeatureResponse>(201);

        group.MapGet("/{mapId:guid}/features", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.GetByMap(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapFeatures")
            .WithDescription("Get all features for a map")
            .RequireAuthorization()
            .Produces<List<MapFeatureResponse>>(200);

        group.MapGet("/{mapId:guid}/features/by-category/{category}", async (
                [FromRoute] Guid mapId,
                [FromRoute] FeatureCategoryEnum category,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.GetByMapAndCategory(mapId, category);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapFeaturesByCategory")
            .WithDescription("Get features for a map filtered by category")
            .RequireAuthorization()
            .Produces<List<MapFeatureResponse>>(200);

        group.MapGet("/{mapId:guid}/features/by-layer/{layerId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid layerId,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.GetByMapAndLayer(mapId, layerId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapFeaturesByLayer")
            .WithDescription("Get features for a map filtered by layer")
            .RequireAuthorization()
            .Produces<List<MapFeatureResponse>>(200);

        group.MapPut("/{mapId:guid}/features/{featureId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid featureId,
                [FromBody] UpdateMapFeatureRequest req,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.Update(featureId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateMapFeature")
            .WithDescription("Update a feature on a map")
            .RequireAuthorization()
            .Produces<MapFeatureResponse>(200);

        group.MapDelete("/{mapId:guid}/features/{featureId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid featureId,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.Delete(featureId);
                return result.Match(
                    success => Results.Ok(new { deleted = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteMapFeature")
            .WithDescription("Delete a feature from a map")
            .RequireAuthorization()
            .Produces(200);
        // Zone/Feature Operations
        group.MapPost("/{mapId:guid}/layers/{sourceLayerId:guid}/copy-feature", async (
                Guid mapId,
                Guid sourceLayerId,
                [FromBody] CopyFeatureToLayerRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.CopyFeatureToLayer(mapId, sourceLayerId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CopyFeatureToLayer")
            .WithDescription("Copy a feature/zone from one layer to another")
            .RequireAuthorization()
            .Produces<CopyFeatureToLayerResponse>(200);

        group.MapDelete("/{mapId:guid}/layers/{layerId:guid}/features/{featureIndex:int}", async (
                Guid mapId,
                Guid layerId,
                int featureIndex,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.DeleteFeatureFromLayer(mapId, layerId, featureIndex);
                return result.Match(
                    success => Results.NoContent(),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteFeatureFromLayer")
            .WithDescription("Delete a feature/zone from a layer")
            .RequireAuthorization()
            .Produces(204);

        group.MapPut("/{mapId:guid}/layers/{layerId:guid}/data", async (
                Guid mapId,
                Guid layerId,
                [FromBody] UpdateLayerDataRequest req,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.UpdateLayerData(mapId, layerId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateLayerData")
            .WithDescription("Update layer's GeoJSON data")
            .RequireAuthorization()
            .Produces<UpdateLayerDataResponse>(200);
    }

    private static async Task<IResult> CreateTemplateHandler(
        IFormFile geoJsonFile,
        [FromForm] string? templateName,
        [FromForm] string? description,
        [FromForm] string? layerName,
        [FromForm] string? category,
        [FromForm] bool isPublic,
        [FromServices] IFileProcessorService fileProcessorService,
        [FromServices] IMapService mapService,
        [FromServices] ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = currentUserService.GetUserId();
            if (currentUserId == null)
            {
                return Results.Unauthorized();
            }

            // Set default values
            templateName ??= "New Template";
            description ??= "";
            layerName ??= "New Layer";
            category ??= "General";

            if (geoJsonFile == null || geoJsonFile.Length == 0)
            {
                return Results.BadRequest(new
                {
                    error = "No file uploaded",
                    message = "Please provide a valid GeoJSON file"
                });
            }

            if (!fileProcessorService.IsSupported(geoJsonFile.FileName))
            {
                return Results.BadRequest(new
                {
                    error = "Unsupported file type",
                    message = "Supported formats: GeoJSON, KML, GPX, CSV, Excel, GeoTIFF"
                });
            }

            var fileSizeMb = geoJsonFile.Length / (1024.0 * 1024.0);
            if (geoJsonFile.Length > 100 * 1024 * 1024)
            {
                return Results.BadRequest(new
                {
                    error = "File too large",
                    message = "File size must be less than 100MB",
                    currentSize = $"{fileSizeMb:F2} MB"
                });
            }

            var processingWarning = "";
            if (fileSizeMb > 10)
            {
                processingWarning = "Large file detected. Processing may take longer than usual.";
            }

            var processedData = await fileProcessorService.ProcessUploadedFile(geoJsonFile, layerName);

            if (!processedData.Success)
            {
                return Results.BadRequest(new
                {
                    error = "File processing failed",
                    message = processedData.ErrorMessage,
                    success = false
                });
            }

            if (!Enum.TryParse<MapTemplateCategoryEnum>(category, true, out var categoryEnum))
            {
                categoryEnum = MapTemplateCategoryEnum.General;
            }

            var result = await mapService.CreateMapTemplateFromGeoJson(new CreateMapTemplateFromGeoJsonRequest
            {
                TemplateName = templateName,
                Description = description,
                Category = categoryEnum,
                IsPublic = isPublic,
                LayerName = layerName,
                GeoJsonData = processedData.LayerData,
                LayerStyle = processedData.LayerStyle,
                DataBounds = processedData.DataBounds,
                FeatureCount = processedData.FeatureCount,
                DataSizeKB = processedData.DataSizeKB
            });

            return result.Match(
                success => Results.Created($"/api/maps/templates/{success.TemplateId}", new
                {
                    templateId = success.TemplateId,
                    message = "MapTemplate created successfully",
                    warning = !string.IsNullOrEmpty(processingWarning) ? processingWarning : null
                }),
                error => error.ToProblemDetailsResult()
            );
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "MapTemplate creation failed",
                detail: ex.Message,
                statusCode: 500
            );
        }

        
    }
}
