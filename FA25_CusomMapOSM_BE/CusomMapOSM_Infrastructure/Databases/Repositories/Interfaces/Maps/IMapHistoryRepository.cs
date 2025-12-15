using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapHistoryRepository
{
    Task<int> AddAsync(MapHistory history, CancellationToken ct = default);
    Task<List<MapHistory>> GetLastAsync(Guid mapId, int maxCount, CancellationToken ct = default);
    Task TrimToAsync(Guid mapId, int keepCount, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default);
}


