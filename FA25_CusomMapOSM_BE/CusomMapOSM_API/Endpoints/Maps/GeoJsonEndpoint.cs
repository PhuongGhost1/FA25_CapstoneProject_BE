using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CusomMapOSM_API.Endpoints.Maps;

public class GeoJsonEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/maps/geojson")
            .WithTags("GeoJSON")
            .WithDescription("GeoJSON processing endpoints");

        group.MapPost("/create-template", async (
                IFormFile geoJsonFile,
                [FromForm] string? templateName,
                [FromForm] string? description,
                [FromForm] string? layerName,
                [FromForm] string? category,
                [FromForm] bool isPublic,
                [FromServices] IFileProcessorService fileProcessorService,
                [FromServices] IMapService mapService,
                [FromServices] ICurrentUserService currentUserService) =>
            {
                try
                {
                    // Get current user
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
                        return Results.BadRequest(new { 
                            error = "No file uploaded", 
                            message = "Please provide a valid GeoJSON file" 
                        });
                    }
                    
                if (!fileProcessorService.IsSupported(geoJsonFile.FileName, geoJsonFile.ContentType))
                {
                    return Results.BadRequest(new {
                        error = "Unsupported file type",
                        message = "Supported formats: GeoJSON, KML, GPX, CSV, Excel, GeoTIFF, PNG, JPG"
                    });
                }

                    var fileSizeMB = geoJsonFile.Length / (1024.0 * 1024.0);
                    if (geoJsonFile.Length > 100 * 1024 * 1024)
                    {
                        return Results.BadRequest(new { 
                            error = "File too large", 
                            message = "File size must be less than 100MB",
                            currentSize = $"{fileSizeMB:F2} MB"
                        });
                    }

                    var processingWarning = "";
                    if (fileSizeMB > 10)
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
                            success = true,
                            message = "MapTemplate created successfully",
                            warning = !string.IsNullOrEmpty(processingWarning) ? processingWarning : null,
                            template = success,
                            layerInfo = new
                            {
                                layerName = layerName,
                                fileName = geoJsonFile.FileName,
                                fileSize = $"{processedData.DataSizeKB} KB",
                                fileSizeMB = $"{fileSizeMB:F2} MB",
                                featureCount = processedData.FeatureCount,
                                geometryType = processedData.GeometryType,
                                propertyNames = processedData.PropertyNames
                            },
                            performance = new
                            {
                                processedAsync = fileSizeMB > 10,
                                compressionApplied = processedData.DataSizeKB > 5000,
                                estimatedProcessingTime = fileSizeMB > 20 ? "2-5 minutes" : 
                                                        fileSizeMB > 10 ? "30-120 seconds" : "< 30 seconds"
                            }
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
            })
            .WithName("CreateMapTemplateFromGeoJson")
            .WithDescription("Create MapTemplate from uploaded GeoJSON file")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .DisableAntiforgery();
    }
}
