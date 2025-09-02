using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapRepository : IMapRepository
{
    private readonly CustomMapOSMDbContext _context;

    public MapRepository(CustomMapOSMDbContext context)
    {
        _context = context;
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

    // Map Template operations
    public async Task<List<Map>> GetMapTemplates()
    {
        return await _context.Maps
            .Include(t => t.User)
            .Where(t => t.IsTemplate && t.IsActive)
            .OrderByDescending(t => t.IsFeatured)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetMapsByCategory(string category)
    {
        return await _context.Maps
            .Include(t => t.User)
            .Where(t => t.IsTemplate && t.IsActive && t.Category.ToString() == category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Map?> GetMapTemplateById(Guid templateId)
    {
        return await _context.Maps
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.MapId == templateId && t.IsTemplate && t.IsActive);
    }
    
    public async Task<MapTemplateWithDetails?> GetMapTemplateWithDetails(Guid templateId)
    {
        var template = await _context.Maps
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.MapId == templateId && t.IsTemplate && t.IsActive);

        if (template == null)
            return null;

        var mapLayersTask = GetTemplateLayers(templateId);
        var mapImagesTask = GetTemplateImages(templateId);

        await Task.WhenAll(mapLayersTask, mapImagesTask);

        var templateWithDetails = new MapTemplateWithDetails
        {
            Map = template,
            MapLayers = mapLayersTask.Result,
            MapImages = mapImagesTask.Result
        };

        return templateWithDetails;
    }
    
    // Template Content operations
    public async Task<List<MapLayer>> GetTemplateLayers(Guid mapId)
    {
        return await _context.MapLayers
            .Include(tl => tl.Layer) // Include Layer navigation property
            .Where(tl => tl.MapId == mapId)
            .OrderBy(tl => tl.LayerOrder)
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
        // First try to find MapLayer by MapId and LayerId
        var mapLayer = await _context.MapLayers
            .Include(ml => ml.Layer)
            .FirstOrDefaultAsync(ml => ml.MapId == mapId && ml.LayerId == layerId);
        
        // If not found, try to find by MapId and check if layerId is actually a MapLayerId
        if (mapLayer == null)
        {
            mapLayer = await _context.MapLayers
                .Include(ml => ml.Layer)
                .FirstOrDefaultAsync(ml => ml.MapId == mapId && ml.MapLayerId == layerId);
        }
        
        if (mapLayer == null)
        {
            // Debug: Check what MapLayers exist for this map
            var allMapLayers = await _context.MapLayers
                .Where(ml => ml.MapId == mapId)
                .Select(ml => new { ml.MapLayerId, ml.LayerId, ml.IsVisible, LayerName = ml.Layer != null ? ml.Layer.LayerName : "null" })
                .ToListAsync();
                
            Console.WriteLine($"MapLayer not found for MapId: {mapId}, LayerId: {layerId}");
            Console.WriteLine($"Available MapLayers for MapId {mapId}:");
            foreach (var ml in allMapLayers)
            {
                Console.WriteLine($"  - MapLayerId:{ml.MapLayerId}, LayerId:{ml.LayerId}, Visible:{ml.IsVisible}, Name:{ml.LayerName}");
            }
            return null;
        }
        
        if (mapLayer.Layer == null)
        {
            Console.WriteLine($"Layer entity is null for MapLayerId: {mapLayer.MapLayerId}");
            return null;
        }
        
        if (string.IsNullOrEmpty(mapLayer.Layer.LayerData))
        {
            Console.WriteLine($"LayerData is empty for LayerId: {mapLayer.LayerId}, LayerName: {mapLayer.Layer.LayerName}");
            return null;
        }
        
        Console.WriteLine($"Successfully found layer data for LayerId: {mapLayer.LayerId}, Size: {mapLayer.Layer.LayerData?.Length ?? 0} chars");
        return mapLayer.Layer.LayerData;
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

    public async Task<bool> CreateLayer(Layer layer)
    {
        try
        {
            _context.Layers.Add(layer);
            
            _context.Database.SetCommandTimeout(300);
            
            var result = await _context.SaveChangesAsync();
            
            // Reset timeout to default
            _context.Database.SetCommandTimeout(30);
            
            return result > 0;
        }
        catch (Exception ex)
        {
            _context.Database.SetCommandTimeout(30);
            Console.WriteLine($"Error saving Layer: {ex.Message}");
            throw;
        }
    }
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
            .FirstOrDefaultAsync(ml => ml.MapId == mapId && ml.LayerId == layerId);
    }

    public async Task<List<MapLayer>> GetMapLayers(Guid mapId)
    {
        return await _context.MapLayers
            .Include(ml => ml.Layer)
                .ThenInclude(l => l.User)
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
