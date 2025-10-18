using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapHistoryRepository : IMapHistoryRepository
{
    private readonly CustomMapOSMDbContext _db;
    public MapHistoryRepository(CustomMapOSMDbContext db)
    {
        _db = db;
    }

    public async Task<int> AddAsync(MapHistory history, CancellationToken ct = default)
    {
        await _db.Set<MapHistory>().AddAsync(history, ct);
        await _db.SaveChangesAsync(ct);
        return history.VersionId;
    }

    public async Task<List<MapHistory>> GetLastAsync(Guid mapId, int maxCount, CancellationToken ct = default)
    {
        return await _db.Set<MapHistory>()
            .AsNoTracking()
            .Where(h => h.MapId == mapId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(maxCount)
            .ToListAsync(ct);
    }

    public async Task TrimToAsync(Guid mapId, int keepCount, CancellationToken ct = default)
    {
        var idsToDelete = await _db.Set<MapHistory>()
            .Where(h => h.MapId == mapId)
            .OrderByDescending(h => h.CreatedAt)
            .Skip(keepCount)
            .Select(h => h.VersionId)
            .ToListAsync(ct);

        if (idsToDelete.Count == 0) return;

        var toRemove = await _db.Set<MapHistory>()
            .Where(h => idsToDelete.Contains(h.VersionId))
            .ToListAsync(ct);

        _db.Set<MapHistory>().RemoveRange(toRemove);
        await _db.SaveChangesAsync(ct);
    }
}


