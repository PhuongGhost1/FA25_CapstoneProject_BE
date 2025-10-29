using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Application.Interfaces.Services.Maps;

public interface IMapHistoryStore
{
    Task AddAsync(MapHistory history, CancellationToken ct = default);
    Task<IReadOnlyCollection<MapHistory>> GetLastAsync(Guid mapId, int maxCount, CancellationToken ct = default);
    Task TrimToAsync(Guid mapId, int keepCount, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default);
}
