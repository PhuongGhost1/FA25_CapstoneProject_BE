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
    
    // Check if a user exists in the database
    public async Task<bool> CheckUserExists(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId);
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
            Layers = mapLayersTask.Result,
            MapImages = mapImagesTask.Result
        };

        return templateWithDetails;
    }
    
    // Template Content operations
    public async Task<List<Layer>> GetTemplateLayers(Guid mapId)
    {
        return await _context.Layers
            .Include(l => l.User) // Include User navigation property
            .Where(l => l.MapId == mapId)
            .OrderBy(l => l.LayerOrder)
            .ToListAsync();
    }



    public async Task<List<MapImage>> GetTemplateImages(Guid mapId)
    {
        return await _context.MapImages
            .Where(ti => ti.MapId == mapId)
            .OrderBy(ti => ti.CreatedAt)
            .ToListAsync();
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
            // Reset timeout on error then bubble up
            _context.Database.SetCommandTimeout(30);
            throw;
        }
    }

    public async Task<string?> GetLayerDataById(Guid mapId, Guid layerId)
    {
        // Find Layer by MapId and LayerId
        var layer = await _context.Layers
            .FirstOrDefaultAsync(l => l.MapId == mapId && l.LayerId == layerId);
        
        if (layer == null)
        {
            // Debug: Check what Layers exist for this map
            var allLayers = await _context.Layers
                .Where(l => l.MapId == mapId)
                .Select(l => new { l.LayerId, l.IsVisible, l.LayerName })
                .ToListAsync();
                
            // no-op
            return null;
        }
        
        if (string.IsNullOrEmpty(layer.LayerData))
        {
            return null;
        }
        
        return layer.LayerData;
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

    public async Task<Layer?> GetLayerById(Guid layerId)
    {
        return await _context.Layers
            .Include(l => l.User)
            .Include(l => l.Map)
            .FirstOrDefaultAsync(l => l.LayerId == layerId);
    }

    public async Task<bool> RemoveLayerFromMap(Guid mapId, Guid layerId)
    {
        var layer = await _context.Layers
            .FirstOrDefaultAsync(l => l.MapId == mapId && l.LayerId == layerId);

        if (layer == null)
            return false;

        _context.Layers.Remove(layer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateLayer(Layer layer)
    {
        _context.Layers.Update(layer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Layer?> GetMapLayer(Guid mapId, Guid layerId)
    {
        return await _context.Layers
            .Include(l => l.User)
            .Include(l => l.Map)
            .FirstOrDefaultAsync(l => l.MapId == mapId && l.LayerId == layerId);
    }

    public async Task<List<Layer>> GetMapLayers(Guid mapId)
    {
        return await _context.Layers
            .Include(l => l.User)
            .Include(l => l.Map)
            .Where(l => l.MapId == mapId)
            .OrderBy(l => l.LayerOrder)
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

