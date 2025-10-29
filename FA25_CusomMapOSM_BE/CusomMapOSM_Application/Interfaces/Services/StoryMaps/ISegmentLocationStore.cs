using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Locations;

namespace CusomMapOSM_Application.Interfaces.Services.StoryMaps;

public interface ISegmentLocationStore
{
    Task<Location?> GetAsync(Guid locationId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetByMapAsync(Guid mapId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetBySegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Location> CreateAsync(Location location, CancellationToken ct = default);
    Task<Location?> UpdateAsync(Location location, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid locationId, CancellationToken ct = default);
}
