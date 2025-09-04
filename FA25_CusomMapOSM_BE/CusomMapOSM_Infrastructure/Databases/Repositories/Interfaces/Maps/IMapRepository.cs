using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapRepository
{
    // Map CRUD operations
    Task<bool> CreateMap(Map map);
    Task<Map?> GetMapById(Guid mapId);
    Task<List<Map>> GetUserMaps(Guid userId);
    Task<List<Map>> GetOrganizationMaps(Guid orgId);
    Task<List<Map>> GetPublicMaps();
    Task<bool> UpdateMap(Map map);
    Task<bool> DeleteMap(Guid mapId);

    // Map Template operations
    Task<List<Map>> GetMapTemplates();
    Task<List<Map>> GetMapsByCategory(string category);
    Task<Map?> GetMapTemplateById(Guid templateId);
    Task<MapTemplateWithDetails?> GetMapTemplateWithDetails(Guid templateId);
    Task<bool> CreateMapTemplate(Map template);
    Task<bool> UpdateMapTemplate(Map template);

    // Template Content operations
    Task<List<MapLayer>> GetTemplateLayers(Guid mapId);
    Task<List<MapImage>> GetTemplateImages(Guid mapId);
    Task<bool> CreateMapTemplateLayer(MapLayer templateLayer);
    Task<bool> CreateLayer(Layer layer);
    Task<string?> GetLayerDataById(Guid mapId, Guid layerId);

    // Map Layer operations
    Task<bool> AddLayerToMap(MapLayer mapLayer);
    Task<bool> RemoveLayerFromMap(Guid mapId, Guid layerId);
    Task<bool> UpdateMapLayer(MapLayer mapLayer);
    Task<MapLayer?> GetMapLayer(Guid mapId, Guid layerId);
    Task<List<MapLayer>> GetMapLayers(Guid mapId);

    // Collaboration operations
    Task<bool> ShareMap(Guid mapId, Guid userId, string permission);
    Task<bool> UnshareMap(Guid mapId, Guid userId);
    Task<List<Map>> GetSharedMaps(Guid userId);
}
