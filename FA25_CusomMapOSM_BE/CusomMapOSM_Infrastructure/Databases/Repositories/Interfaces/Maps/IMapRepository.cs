using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapRepository
{
    // User validation
    Task<bool> CheckUserExists(Guid userId);
    
    // Map CRUD operations
    Task<bool> CreateMap(Map map);
    Task<Map?> GetMapById(Guid mapId);
    Task<List<Map>> GetUserMaps(Guid userId);
    Task<List<Map>> GetOrganizationMaps(Guid orgId);
    Task<List<Map>> GetByWorkspaceIdAsync(Guid workspaceId);
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
    Task<List<Layer>> GetTemplateLayers(Guid mapId);
    Task<List<MapImage>> GetTemplateImages(Guid mapId);
    Task<bool> CreateLayer(Layer layer);

    // Map Layer operations
    Task<Layer?> GetLayerById(Guid layerId);
    Task<bool> RemoveLayerFromMap(Guid mapId, Guid layerId);
    Task<bool> UpdateLayer(Layer layer);
    Task<Layer?> GetMapLayer(Guid mapId, Guid layerId);
    Task<List<Layer>> GetMapLayers(Guid mapId);
    Task<List<MapFeature>> GetMapFeatures(Guid mapId);

    // Collaboration operations
    Task<bool> ShareMap(Guid mapId, Guid userId, string permission);
    Task<bool> UnshareMap(Guid mapId, Guid userId);
    Task<List<Map>> GetSharedMaps(Guid userId);
    
    // Statistics operations
    Task<int> GetTotalMapsCount();
    Task<int> GetMonthlyExportsCount();

    // Custom listings
    Task<List<Map>> GetUserDraftMaps(Guid userId);
    Task<List<Map>> GetUserRecentMaps(Guid userId, int limit);
    Task<List<(Map Map, DateTime LastActivity)>> GetUserRecentMapsWithActivity(Guid userId, int limit);
}
