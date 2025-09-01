using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
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

        group.MapGet("/organization/{orgId}", async (
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

        group.MapGet("/templates/{templateId}", async (
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

        group.MapGet("/templates/{templateId}/layers/{layerId}/data", async (
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

        group.MapGet("/{mapId}", async (
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

        group.MapPut("/{mapId}", async (
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

        group.MapDelete("/{mapId}", async (
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
        
        group.MapPost("/{mapId}/layers", async (
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

        group.MapDelete("/{mapId}/layers/{layerId}", async (
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

        group.MapPut("/{mapId}/layers/{layerId}", async (
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
        
        group.MapPost("/{mapId}/share", async (
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

        group.MapDelete("/{mapId}/share", async (
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
    }
}
