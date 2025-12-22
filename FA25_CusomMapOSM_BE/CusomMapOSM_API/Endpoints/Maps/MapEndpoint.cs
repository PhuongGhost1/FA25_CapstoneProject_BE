using System.Text.Json;
using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_API.Endpoints.Maps;

public class MapEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Maps)
            .WithTags(Tags.Map)
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

        group.MapGet("/my/recents", async (
                [FromServices] IMapService mapService,
                [FromQuery] int? limit) =>
            {
                var result = await mapService.GetMyRecentMaps(limit ?? 20);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyRecentMaps")
            .WithDescription(
                "Get recent maps owned by current user ordered by last activity (layers/features/images/history)")
            .RequireAuthorization()
            .Produces<GetMyMapsResponse>(200);

        group.MapGet("/my/drafts", async (
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.GetMyDraftMaps();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyDraftMaps")
            .WithDescription("Get draft maps owned by current user")
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
                    success => Results.Ok(new LayerDataResponse { LayerData = success }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapTemplateLayerData")
            .WithDescription("Get layer GeoJSON data for map template")
            .AllowAnonymous()
            .Produces<LayerDataResponse>(200)
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
            .AllowAnonymous()
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

        // ===== Map Publishing =====
        group.MapPost("/{mapId:guid}/publish", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.PublishMap(mapId);
                return result.Match(
                    success => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("PublishMap")
            .WithDescription(
                "Publish a map. Set IsStoryMap=true to publish as storymap (can create sessions), false for view-only.")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapPost("/{mapId:guid}/unpublish", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.UnpublishMap(mapId);
                return result.Match(
                    success => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UnpublishMap")
            .WithDescription("Unpublish a map")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapPost("/{mapId:guid}/prepare-embed", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.PrepareForEmbed(mapId);
                return result.Match(
                    success => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("PrepareForEmbed")
            .WithDescription("Automatically publish and set map to public for embedding")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapPost("/{mapId:guid}/archive", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.ArchiveMap(mapId);
                return result.Match(
                    success => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ArchiveMap")
            .WithDescription("Archive a map")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapPost("/{mapId:guid}/restore", async (
                [FromRoute] Guid mapId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.RestoreMap(mapId);
                return result.Match(
                    success => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("RestoreMap")
            .WithDescription("Restore an archived map to Draft")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

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

        group.MapPost("/template", CreateTemplateHandler)
            .WithName("CreateMapTemplateFromGeoJson")
            .WithDescription("Create MapTemplate from uploaded GeoJSON file")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .DisableAntiforgery();

        // Upload GeoJSON file to existing map
        group.MapPost("/{mapId:guid}/upload-geojson", async (
                [FromRoute] Guid mapId,
                IFormFile file,
                [FromForm] string? layerName,
                [FromServices] IFileProcessorService fileProcessorService,
                [FromServices] IMapService mapService,
                [FromServices] IMapFeatureService featureService,
                [FromServices] IServiceScopeFactory serviceScopeFactory,
                [FromServices] ILogger<MapEndpoints> logger,
                [FromServices] ICurrentUserService currentUserService,
                [FromServices] CusomMapOSM_Application.Interfaces.Services.LayerData.ILayerDataStore layerDataStore,
                CancellationToken ct) =>
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new
                    {
                        error = "No file uploaded",
                        message = "Please provide a valid GeoJSON file"
                    });
                }

                if (!fileProcessorService.IsSupported(file.FileName))
                {
                    return Results.BadRequest(new
                    {
                        error = "Unsupported file type",
                        message = "Supported formats: GeoJSON, KML, GPX, CSV, Excel, GeoTIFF"
                    });
                }

                if (file.Length > 100 * 1024 * 1024)
                {
                    return Results.BadRequest(new
                    {
                        error = "File too large",
                        message = "File size must be less than 100MB"
                    });
                }

                var processedData = await fileProcessorService.ProcessUploadedFile(file, layerName ?? "Uploaded Layer");
                if (!processedData.Success)
                {
                    return Results.BadRequest(new
                    {
                        error = "File processing failed",
                        message = processedData.ErrorMessage
                    });
                }

                var addLayerResult = await mapService.AddLayerToMap(mapId, new AddLayerToMapRequest
                {
                    LayerName = layerName ?? "Uploaded Layer",
                    LayerData = processedData.LayerData,
                    LayerTypeId = processedData.LayerType.ToString(),
                    IsVisible = true,
                    ZIndex = 1
                });

                if (!addLayerResult.HasValue)
                    return addLayerResult.Match(_ => throw new InvalidOperationException(),
                        e => e.ToProblemDetailsResult());

                var success = addLayerResult.ValueOr(() => new AddLayerToMapResponse { MapLayerId = Guid.Empty });
                if (success.MapLayerId == Guid.Empty)
                    return Results.Problem("Failed to create map layer");

                var featuresCreated = 0;
                var featuresFailed = 0;
                var featuresSkipped = 0;

                var processedFeatureIds = new HashSet<string>();
                var existingFeaturesResult = await featureService.GetByMap(mapId);
                var existingFeatureIds = existingFeaturesResult.HasValue
                    ? existingFeaturesResult.Match(s => s.Select(x => x.FeatureId.ToString()).ToHashSet(),
                        _ => new HashSet<string>())
                    : new HashSet<string>();

                JsonDocument? geoJsonDoc = null;

                try
                {
                    geoJsonDoc = JsonDocument.Parse(processedData.LayerData!);

                    if (!geoJsonDoc.RootElement.TryGetProperty("features", out var features) ||
                        features.ValueKind != JsonValueKind.Array)
                    {
                        return Results.BadRequest("Invalid GeoJSON: missing features array");
                    }

                    foreach (var feature in features.EnumerateArray())
                    {
                        try
                        {
                            string? featureId = feature.TryGetProperty("id", out var idEl)
                                ? idEl.ToString()
                                : null;

                            if (!string.IsNullOrEmpty(featureId))
                            {
                                if (!processedFeatureIds.Add(featureId) || existingFeatureIds.Contains(featureId))
                                {
                                    featuresSkipped++;
                                    continue;
                                }
                            }

                            if (!feature.TryGetProperty("geometry", out var geometry) ||
                                geometry.ValueKind == JsonValueKind.Null)
                            {
                                featuresFailed++;
                                continue;
                            }

                            if (!geometry.TryGetProperty("type", out var geomTypeEl) ||
                                geomTypeEl.ValueKind != JsonValueKind.String)
                            {
                                throw new NotSupportedException("Geometry type missing");
                            }

                            var geomTypeStr = geomTypeEl.GetString()!.Trim().ToUpperInvariant();

                            var geometryType = geomTypeStr switch
                            {
                                "POINT" => GeometryTypeEnum.Point,
                                "MULTIPOINT" => GeometryTypeEnum.MultiPoint,
                                "LINESTRING" => GeometryTypeEnum.LineString,
                                "MULTILINESTRING" => GeometryTypeEnum.MultiLineString,
                                "POLYGON" => GeometryTypeEnum.Polygon,
                                "MULTIPOLYGON" => GeometryTypeEnum.MultiPolygon,
                                _ => throw new NotSupportedException($"Unsupported geometry type: {geomTypeStr}")
                            };

                            var properties = feature.TryGetProperty("properties", out var p) ? p : default;

                            var name = properties.TryGetProperty("name", out var n)
                                ? n.GetString() ?? "Unnamed Feature"
                                : "Unnamed Feature";
                            var description = properties.TryGetProperty("description", out var d)
                                ? d.GetString() ?? ""
                                : "";

                            var featureCategory = properties.TryGetProperty("featureCategory", out var fc) &&
                                                  Enum.TryParse<FeatureCategoryEnum>(fc.GetString(), true, out var cat)
                                ? cat
                                : FeatureCategoryEnum.Data;

                            var annotationType = properties.TryGetProperty("annotationType", out var at) &&
                                                 Enum.TryParse<AnnotationTypeEnum>(at.GetString(), true, out var ann)
                                ? ann
                                : (AnnotationTypeEnum?)null;

                            if (annotationType == AnnotationTypeEnum.Text)
                            {
                                featuresSkipped++;
                                continue;
                            }

                            var zIndex = properties.TryGetProperty("zIndex", out var zi) && zi.TryGetInt32(out var z)
                                ? z
                                : 0;

                            var extraProps = new Dictionary<string, object>();
                            if (properties.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in properties.EnumerateObject())
                                {
                                    if (prop.Name is "name" or "description" or "featureCategory" or "annotationType"
                                        or "zIndex")
                                        continue;

                                    extraProps[prop.Name] =
                                        JsonSerializer.Deserialize<object>(prop.Value.GetRawText()) ??
                                        prop.Value.GetRawText();
                                }
                            }

                            var createResult = await featureService.Create(new CreateMapFeatureRequest
                            {
                                MapId = mapId,
                                LayerId = success.MapLayerId,
                                Name = name,
                                Description = description,
                                FeatureCategory = featureCategory,
                                AnnotationType = annotationType,
                                GeometryType = geometryType,
                                Coordinates = geometry.GetRawText(),
                                Properties = extraProps.Count > 0 ? JsonSerializer.Serialize(extraProps) : null,
                                IsVisible = true,
                                ZIndex = zIndex
                            });

                            if (createResult.HasValue)
                                featuresCreated++;
                            else
                                featuresFailed++;
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to create feature from GeoJSON: {GeometryType}", 
                                feature.TryGetProperty("geometry", out var g) && g.TryGetProperty("type", out var t) 
                                    ? t.GetString() 
                                    : "unknown");
                            featuresFailed++;
                        }
                    }

                    using var scope = serviceScopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IMapRepository>();
                    var layer = await repo.GetMapLayer(mapId, success.MapLayerId);

                    if (layer != null)
                    {
                        var cleaned = JsonSerializer.Serialize(new
                        {
                            type = "FeatureCollection",
                            features = Array.Empty<object>()
                        });

                        await layerDataStore.SetDataAsync(layer, cleaned);
                    }
                }
                finally
                {
                    geoJsonDoc?.Dispose();
                }

                return Results.Ok(new
                {
                    layerId = success.MapLayerId,
                    featuresCreated,
                    featuresFailed,
                    featuresSkipped,
                    totalFeaturesInFile = processedData.FeatureCount
                });
            })
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200);


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
            .AllowAnonymous()
            .Produces<List<MapFeatureResponse>>(200);

        group.MapGet("/{mapId:guid}/features/{featureId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid featureId,
                [FromServices] IMapFeatureService featureService) =>
            {
                var result = await featureService.GetById(featureId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMapFeatureById")
            .WithDescription("Get a specific feature by ID")
            .RequireAuthorization()
            .AllowAnonymous()
            .Produces<MapFeatureResponse>(200)
            .ProducesProblem(404);

        // Undo map history (up to last 10 snapshots)
        group.MapPost("/{mapId:guid}/history/undo", async (
                [FromRoute] Guid mapId,
                [FromQuery] int steps,
                [FromServices] IMapHistoryService historyService,
                [FromServices] IMapService mapService) =>
            {
                var result =
                    await historyService.Undo(mapId, Guid.Empty, steps); // userId not required for undo retrieval
                return result.Match(
                    success => Results.Ok(new MapSnapshotResponse { Snapshot = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UndoMapHistory")
            .WithDescription("Get a prior snapshot of the map by stepping back N (<=10)")
            .RequireAuthorization()
            .Produces<MapSnapshotResponse>(200)
            .Produces(400);

        group.MapPost("/{mapId:guid}/history/apply", async (
                [FromRoute] Guid mapId,
                [FromBody] string snapshot,
                [FromServices] IMapFeatureService featureService,
                [FromServices] IMapService mapService) =>
            {
                var result = await featureService.ApplySnapshot(mapId, snapshot);
                return result.Match(
                    success => Results.Ok(new ApplySnapshotResponse { Applied = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ApplyMapHistorySnapshot")
            .WithDescription("Apply a provided snapshot JSON to restore map features")
            .RequireAuthorization()
            .Produces<ApplySnapshotResponse>(200)
            .Produces(400);

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
                    success => Results.Ok(new DeleteFeatureResponse { Deleted = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteMapFeature")
            .WithDescription("Delete a feature from a map")
            .RequireAuthorization()
            .Produces<DeleteFeatureResponse>(200);
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

        // Move Map to Workspace
        group.MapPost("/{mapId:guid}/move-to-workspace/{workspaceId:guid}", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid workspaceId,
                [FromServices] IMapService mapService) =>
            {
                var result = await mapService.MoveMapToWorkspace(mapId, workspaceId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("MoveMapToWorkspace")
            .WithDescription("Move a map to a different workspace (required for creating sessions)")
            .RequireAuthorization()
            .Produces<MoveMapToWorkspaceResponse>(200);
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