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
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m => m.MapId == mapId && m.IsActive);
    }

    public async Task<List<Map>> GetUserMaps(Guid userId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
            .Where(m => m.UserId == userId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetOrganizationMaps(Guid orgId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
            .ThenInclude(w => w!.Organization)
            .Where(m => m.Workspace != null && m.Workspace.Organization != null && m.Workspace.Organization.OrgId == orgId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
            .Where(m => m.WorkspaceId == workspaceId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Map>> GetPublicMaps()
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
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
            .OrderBy(l => l.CreatedAt)
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
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MapFeature>> GetMapFeatures(Guid mapId)
    {
        return await _context.MapFeatures
            .Where(f => f.MapId == mapId)
            .OrderBy(f => f.ZIndex)
            .ThenBy(f => f.CreatedAt)
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

    public async Task<int> GetTotalMapsCount()
    {
        return await _context.Maps
            .Where(m => !m.IsTemplate)
            .CountAsync();
    }

    public async Task<int> GetMonthlyExportsCount()
    {
        // Get the first day of current month
        var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        // Check if MapExports table exists and has data
        // If you don't have MapExports tracking yet, return 0
        try
        {
            // If you have a MapExports DbSet, uncomment this:
            // return await _context.MapExports
            //     .Where(e => e.CreatedAt >= currentMonth)
            //     .CountAsync();
            
            // For now, return 0 until MapExports tracking is implemented
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    // Custom listings
    public async Task<List<Map>> GetUserDraftMaps(Guid userId)
    {
        return await _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
            .Where(m => m.UserId == userId && m.IsActive && m.Status == CusomMapOSM_Domain.Entities.Maps.Enums.MapStatusEnum.Draft)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<(Map Map, DateTime LastActivity)>> GetUserRecentMapsWithActivity(Guid userId, int limit)
    {
        var baseQuery = _context.Maps
            .Include(m => m.User)
            .Include(m => m.Workspace)
            .Where(m => m.UserId == userId && m.IsActive && !m.IsTemplate);

        var mapIds = await baseQuery.Select(m => m.MapId).ToListAsync();

        if (mapIds.Count == 0) return new List<(Map, DateTime)>();

        var layerMax = await _context.Layers
            .Where(l => mapIds.Contains(l.MapId))
            .GroupBy(l => l.MapId)
            .Select(g => new { MapId = g.Key, MaxAt = g.Max(l => l.UpdatedAt ?? l.CreatedAt) })
            .ToDictionaryAsync(x => x.MapId, x => x.MaxAt);

        var historyMax = await _context.MapHistories
            .Where(h => mapIds.Contains(h.MapId))
            .GroupBy(h => h.MapId)
            .Select(g => new { MapId = g.Key, MaxAt = g.Max(h => h.CreatedAt) })
            .ToDictionaryAsync(x => x.MapId, x => x.MaxAt);

        var imageMax = await _context.MapImages
            .Where(i => mapIds.Contains(i.MapId))
            .GroupBy(i => i.MapId)
            .Select(g => new { MapId = g.Key, MaxAt = g.Max(i => i.CreatedAt) })
            .ToDictionaryAsync(x => x.MapId, x => x.MaxAt);

        var featureMax = await _context.MapFeatures
            .Where(f => mapIds.Contains(f.MapId))
            .GroupBy(f => f.MapId)
            .Select(g => new { MapId = g.Key, MaxAt = g.Max(f => f.UpdatedAt ?? f.CreatedAt) })
            .ToDictionaryAsync(x => x.MapId, x => x.MaxAt);

        var maps = await baseQuery.ToListAsync();

        var ordered = maps
            .Select(m =>
            {
                var baseAt = m.UpdatedAt ?? m.CreatedAt;
                var lAt = layerMax.TryGetValue(m.MapId, out var v1) ? v1 : DateTime.MinValue;
                var hAt = historyMax.TryGetValue(m.MapId, out var v2) ? v2 : DateTime.MinValue;
                var iAt = imageMax.TryGetValue(m.MapId, out var v3) ? v3 : DateTime.MinValue;
                var fAt = featureMax.TryGetValue(m.MapId, out var v4) ? v4 : DateTime.MinValue;
                var last = new[] { baseAt, lAt, hAt, iAt, fAt }.Max();
                return (Map: m, LastActivity: last);
            })
            .OrderByDescending(x => x.LastActivity)
            .Take(Math.Max(1, limit))
            .ToList();

        return ordered;
    }
    
    public async Task<List<Map>> GetUserRecentMaps(Guid userId, int limit)
    {
        var results = await GetUserRecentMapsWithActivity(userId, limit);
        return results.Select(x => x.Map).ToList();
    }
}

