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
        history.HistoryId = Guid.NewGuid();
        
        // Get next version for this map
        var maxVersion = await _db.Set<MapHistory>()
            .Where(h => h.MapId == history.MapId)
            .MaxAsync(h => (int?)h.HistoryVersion, ct) ?? 0;
        
        history.HistoryVersion = maxVersion + 1;
        
        await _db.Set<MapHistory>().AddAsync(history, ct);
        await _db.SaveChangesAsync(ct);
        return history.HistoryVersion;
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
        const int maxRetries = 2; // Chỉ retry 2 lần thôi
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                var idsToDelete = await _db.Set<MapHistory>()
                    .Where(h => h.MapId == mapId)
                    .OrderByDescending(h => h.CreatedAt)
                    .Skip(keepCount)
                    .Select(h => h.HistoryId)
                    .ToListAsync(ct);

                if (idsToDelete.Count == 0) return;

                var toRemove = await _db.Set<MapHistory>()
                    .Where(h => idsToDelete.Contains(h.HistoryId))
                    .ToListAsync(ct);

                _db.Set<MapHistory>().RemoveRange(toRemove);
                await _db.SaveChangesAsync(ct);
                return; // Success
            }
            catch (DbUpdateConcurrencyException) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                await Task.Delay(50, ct); // Delay ngắn
            }
            catch (DbUpdateConcurrencyException)
            {
                // Final retry failed, ignore - history trimming is not critical
                return;
            }
        }
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default)
    {
        var oldRecords = await _db.Set<MapHistory>()
            .Where(h => h.CreatedAt < cutoffUtc)
            .ToListAsync(ct);

        if (oldRecords.Count == 0)
        {
            return 0;
        }

        _db.Set<MapHistory>().RemoveRange(oldRecords);
        await _db.SaveChangesAsync(ct);
        return oldRecords.Count;
    }
}
