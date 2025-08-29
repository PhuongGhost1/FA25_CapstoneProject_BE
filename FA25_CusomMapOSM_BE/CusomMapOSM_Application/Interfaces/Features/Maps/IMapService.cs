using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapService
{
    // Map CRUD Operations
    Task<Option<CreateMapResponse, Error>> Create(CreateMapRequest req);
    Task<Option<CreateMapFromTemplateResponse, Error>> CreateFromTemplate(CreateMapFromTemplateRequest req);
    Task<Option<GetMapByIdResponse, Error>> GetById(Guid mapId);
    Task<Option<GetMyMapsResponse, Error>> GetMyMaps();
    Task<Option<GetOrganizationMapsResponse, Error>> GetOrganizationMaps(Guid orgId);
    Task<Option<UpdateMapResponse, Error>> Update(Guid mapId, UpdateMapRequest req);
    Task<Option<DeleteMapResponse, Error>> Delete(Guid mapId);

    // Map Template Operations
    Task<Option<GetMapTemplatesResponse, Error>> GetTemplates();
    Task<Option<GetMapTemplateByIdResponse, Error>> GetTemplateById(Guid templateId);

    // Map Layer Operations
    Task<Option<AddLayerToMapResponse, Error>> AddLayerToMap(Guid mapId, AddLayerToMapRequest req);
    Task<Option<RemoveLayerFromMapResponse, Error>> RemoveLayerFromMap(Guid mapId, Guid layerId);
    Task<Option<UpdateMapLayerResponse, Error>> UpdateMapLayer(Guid mapId, Guid layerId, UpdateMapLayerRequest req);

    // Map Collaboration
    Task<Option<ShareMapResponse, Error>> ShareMap(ShareMapRequest req);
    Task<Option<UnshareMapResponse, Error>> UnshareMap(UnshareMapRequest req);
    
    // Map Template Creation from GeoJSON
    Task<Option<CreateMapTemplateFromGeoJsonResponse, Error>> CreateMapTemplateFromGeoJson(CreateMapTemplateFromGeoJsonRequest req);
    Task<Option<string, Error>> GetLayerData(Guid templateId, Guid layerId);
}
