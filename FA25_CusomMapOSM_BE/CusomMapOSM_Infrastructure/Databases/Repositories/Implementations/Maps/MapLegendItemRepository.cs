using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapLegendItemRepository : IMapLegendItemRepository
{
    private readonly CustomMapOSMDbContext _context;

    public MapLegendItemRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<List<MapLegendItem>> GetByMapIdAsync(Guid mapId, CancellationToken ct = default)
    {
        return await _context.MapLegendItems
            .Where(x => x.MapId == mapId && x.IsVisible)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MapLegendItem?> GetByIdAsync(Guid legendItemId, CancellationToken ct = default)
    {
        return await _context.MapLegendItems
            .FirstOrDefaultAsync(x => x.LegendItemId == legendItemId, ct);
    }

    public async Task<bool> CreateAsync(MapLegendItem item, CancellationToken ct = default)
    {
        _context.MapLegendItems.Add(item);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(MapLegendItem item, CancellationToken ct = default)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.MapLegendItems.Update(item);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(Guid legendItemId, CancellationToken ct = default)
    {
        var item = await _context.MapLegendItems.FindAsync([legendItemId], ct);
        if (item == null) return false;
        
        _context.MapLegendItems.Remove(item);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> ReorderAsync(Guid mapId, List<Guid> itemIds, CancellationToken ct = default)
    {
        var items = await _context.MapLegendItems
            .Where(x => x.MapId == mapId && itemIds.Contains(x.LegendItemId))
            .ToListAsync(ct);

        if (items.Count != itemIds.Count) return false;

        for (int i = 0; i < itemIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.LegendItemId == itemIds[i]);
            if (item != null)
            {
                item.DisplayOrder = i;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> GetMaxDisplayOrderAsync(Guid mapId, CancellationToken ct = default)
    {
        var maxOrder = await _context.MapLegendItems
            .Where(x => x.MapId == mapId)
            .MaxAsync(x => (int?)x.DisplayOrder, ct);
        
        return maxOrder ?? -1;
    }
}
