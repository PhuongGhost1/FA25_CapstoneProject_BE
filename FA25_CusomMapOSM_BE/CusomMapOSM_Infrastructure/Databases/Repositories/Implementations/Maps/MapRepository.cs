using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapRepository : IMapRepository
{
    private readonly CustomMapOSMDbContext _context;
    private readonly ICacheService _cacheService;

    public MapRepository(CustomMapOSMDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    // Map CRUD operations
    public async Task<bool> CreateMap(Map map)
    {
        _context.Maps.Add(map);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Map?> GetMapById(Guid mapId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.MapId == mapId && m.IsActive);
    }

    public async Task<List<Map>> GetUserMaps(Guid userId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.UserId == userId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetOrganizationMaps(Guid orgId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.OrgId == orgId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetPublicMaps()
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.IsPublic && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateMap(Map map)
    {
        _context.Maps.Update(map);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteMap(Guid mapId)
    {
        var map = await _context.Maps.FirstOrDefaultAsync(m => m.MapId == mapId);
        if (map == null)
            return false;

        map.IsActive = false;
        _context.Maps.Update(map);
        return await _context.SaveChangesAsync() > 0;
    }

    // Map Template operations - WITH CACHING
    public async Task<List<Map>> GetMapTemplates()
    {
        // Cache key: "templates:all"
        var cacheKey = "templates:all";

        // Try get from cache first
        var cachedTemplates = await _cacheService.GetAsync<List<Map>>(cacheKey);
        if (cachedTemplates != null)
        {
            return cachedTemplates;
        }

        // Cache miss - query database
        var templates = await _context.Maps
            .Include(t => t.User)
            .Where(t => t.IsTemplate && t.IsActive)
            .OrderByDescending(t => t.IsFeatured)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, templates, TimeSpan.FromMinutes(30));

        return templates;
    }

    public async Task<List<Map>> GetMapsByCategory(string category)
    {
        var cacheKey = $"templates:category:{category}";

        // Try get from cache first
        var cachedTemplates = await _cacheService.GetAsync<List<Map>>(cacheKey);
        if (cachedTemplates != null)
        {
            return cachedTemplates;
        }

        // Cache miss - query database
        var templates = await _context.Maps
            .Include(t => t.User)
            .Where(t => t.IsTemplate && t.IsActive && t.Category.ToString() == category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, templates, TimeSpan.FromMinutes(30));

        return templates;
    }

    public async Task<Map?> GetMapTemplateById(Guid templateId)
    {
        var cacheKey = $"templates:id:{templateId}";

        // Try get from cache first
        var cachedTemplate = await _cacheService.GetAsync<Map>(cacheKey);
        if (cachedTemplate != null)
        {
            return cachedTemplate;
        }

        // Cache miss - query database
        var template = await _context.Maps
            .Include(t => t.User)
            .Include(t => t.MapLayers)
            .Include(t => t.MapAnnotations)
            .Include(t => t.MapImages)
            .FirstOrDefaultAsync(t => t.MapId == templateId && t.IsTemplate && t.IsActive);

        if (template != null)
        {
            await _cacheService.SetAsync(cacheKey, template, TimeSpan.FromMinutes(30));
        }

        return template;
    }
    
    // Template Content operations
    public async Task<List<MapLayer>> GetTemplateLayers(Guid mapId)
    {
        return await _context.MapLayers
            .Where(tl => tl.MapId == mapId)
            .OrderBy(tl => tl.LayerOrder)
            .ToListAsync();
    }

    public async Task<List<MapAnnotation>> GetTemplateAnnotations(Guid mapId)
    {
        return await _context.MapAnnotations
            .Where(ta => ta.MapId == mapId)
            .OrderBy(ta => ta.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MapImage>> GetTemplateImages(Guid mapId)
    {
        return await _context.MapImages
            .Where(ti => ti.MapId == mapId)
            .OrderBy(ti => ti.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CreateMapLayer(MapLayer templateLayer)
    {
        try
        {
            _context.MapLayers.Add(templateLayer);
            
            _context.Database.SetCommandTimeout(300);
            
            var result = await _context.SaveChangesAsync();
            
            // Reset timeout to default
            _context.Database.SetCommandTimeout(30);
            
            return result > 0;
        }
        catch (Exception ex)
        {
            // Reset timeout on error
            _context.Database.SetCommandTimeout(30);
            
            // Log the error for debugging
            Console.WriteLine($"Error saving MapLayer: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetLayerDataById(Guid mapId, Guid layerId)
    {
        var layer = await _context.MapLayers
            .Where(l => l.MapId == mapId && l.MapLayerId == layerId && l.IsVisible)
            .Select(l => l.LayerData)
            .FirstOrDefaultAsync();

        return layer;
    }

    // Template Management operations
    public async Task<bool> CreateMapTemplate(Map template)
    {
        template.IsTemplate = true; // Ensure it's marked as template
        _context.Maps.Add(template);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateMapTemplate(Map template)
    {
        _context.Maps.Update(template);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CreateMapTemplateLayer(MapLayer templateLayer)
    {
        return await CreateMapLayer(templateLayer);
    }

    // Map Layer operations
    public async Task<bool> AddLayerToMap(MapLayer mapLayer)
    {
        _context.MapLayers.Add(mapLayer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveLayerFromMap(Guid mapId, Guid layerId)
    {
        var mapLayer = await _context.MapLayers
            .FirstOrDefaultAsync(ml => ml.MapId == mapId && ml.LayerId == layerId);

        if (mapLayer == null)
            return false;

        _context.MapLayers.Remove(mapLayer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateMapLayer(MapLayer mapLayer)
    {
        _context.MapLayers.Update(mapLayer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<MapLayer?> GetMapLayer(Guid mapId, Guid layerId)
    {
        return await _context.MapLayers
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.User)
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.LayerType)
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.Source)
            .FirstOrDefaultAsync(ml => ml.MapId == mapId && ml.LayerId == layerId);
    }

    public async Task<List<MapLayer>> GetMapLayers(Guid mapId)
    {
        return await _context.MapLayers
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.User)
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.LayerType)
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.Source)
            .Where(ml => ml.MapId == mapId)
            .OrderBy(ml => ml.LayerOrder)
            .ToListAsync();
    }

    // Collaboration operations
    public async Task<bool> ShareMap(Guid mapId, Guid userId, string permission)
    {
        // TODO: Implement collaboration logic when Collaboration entity is ready
        // For now, return true to indicate success
        return true;
    }

    public async Task<bool> UnshareMap(Guid mapId, Guid userId)
    {
        // TODO: Implement unshare logic when Collaboration entity is ready
        // For now, return true to indicate success
        return true;
    }

    public async Task<List<Map>> GetSharedMaps(Guid userId)
    {
        // TODO: Implement shared maps logic when Collaboration entity is ready
        // For now, return empty list
        return new List<Map>();
    }
}
