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
    Task<Option<GetMapTemplateWithDetailsResponse, Error>> GetTemplateWithDetails(Guid templateId);

    // Map Layer Operations
    Task<Option<AddLayerToMapResponse, Error>> AddLayerToMap(Guid mapId, AddLayerToMapRequest req);
    Task<Option<RemoveLayerFromMapResponse, Error>> RemoveLayerFromMap(Guid mapId, Guid layerId);
    Task<Option<UpdateMapLayerResponse, Error>> UpdateMapLayer(Guid mapId, Guid layerId, UpdateMapLayerRequest req);
    
    // Map Template Creation from GeoJSON
    Task<Option<CreateMapTemplateFromGeoJsonResponse, Error>> CreateMapTemplateFromGeoJson(CreateMapTemplateFromGeoJsonRequest req);
    Task<Option<string, Error>> GetLayerData(Guid templateId, Guid layerId);
    
    // Zone/Feature Operations
    Task<Option<CopyFeatureToLayerResponse, Error>> CopyFeatureToLayer(Guid mapId, Guid sourceLayerId, CopyFeatureToLayerRequest req);
    Task<Option<bool, Error>> DeleteFeatureFromLayer(Guid mapId, Guid layerId, int featureIndex);
    Task<Option<UpdateLayerDataResponse, Error>> UpdateLayerData(Guid mapId, Guid layerId, UpdateLayerDataRequest req);
    
    // Layer Operations
    Task<Option<List<LayerInfoResponse>, Error>> GetMapLayers(Guid mapId);
    
    // Map Publishing Operations
    Task<Option<bool, Error>> PublishMap(Guid mapId);
    Task<Option<bool, Error>> UnpublishMap(Guid mapId);
    Task<Option<bool, Error>> ArchiveMap(Guid mapId);
    Task<Option<bool, Error>> RestoreMap(Guid mapId);

    // Custom listings
    Task<Option<GetMyMapsResponse, Error>> GetMyRecentMaps(int limit);
    Task<Option<GetMyMapsResponse, Error>> GetMyDraftMaps();
}
