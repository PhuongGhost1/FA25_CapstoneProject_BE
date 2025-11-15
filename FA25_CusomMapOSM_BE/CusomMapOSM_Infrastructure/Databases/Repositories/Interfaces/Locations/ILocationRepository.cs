using CusomMapOSM_Domain.Entities.Locations;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid locationId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetByMapIdAsync(Guid mapId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetBySegmentIdAsync(Guid segmentId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetByZoneIdAsync(Guid zoneId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Location>> GetWithoutZoneAsync(Guid segmentId, CancellationToken ct = default);
    Task<Location> CreateAsync(Location location, CancellationToken ct = default);
    Task<Location?> UpdateAsync(Location location, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid locationId, CancellationToken ct = default);
}
