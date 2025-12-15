using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Locations;

public class LocationRepository : ILocationRepository
{
    private readonly CustomMapOSMDbContext _context;

    public LocationRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(Guid locationId, CancellationToken ct = default)
    {
        return await _context.MapLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);
    }

    public async Task<IReadOnlyCollection<Location>> GetByMapIdAsync(Guid mapId, CancellationToken ct = default)
    {
        return await _context.MapLocations
            .AsNoTracking()
            .Where(l => l.MapId == mapId)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Location>> GetBySegmentIdAsync(Guid segmentId, CancellationToken ct = default)
    {
        return await _context.MapLocations
            .AsNoTracking()
            .Where(l => l.SegmentId == segmentId)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Location>> GetBySegmentIdsAsync(List<Guid> segmentIds, CancellationToken ct = default)
    {
        if (segmentIds == null || segmentIds.Count == 0)
        {
            return Array.Empty<Location>();
        }

        return await _context.MapLocations
            .AsNoTracking()
            .Where(l => l.SegmentId.HasValue && segmentIds.Contains(l.SegmentId.Value))
            .OrderBy(l => l.SegmentId)
            .ThenBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Location>> GetByZoneIdAsync(Guid zoneId, CancellationToken ct = default)
    {
        return await _context.MapLocations
            .AsNoTracking()
            .Include(l => l.Zone)
            .Where(l => l.ZoneId == zoneId)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Location>> GetWithoutZoneAsync(Guid segmentId, CancellationToken ct = default)
    {
        return await _context.MapLocations
            .AsNoTracking()
            .Where(l => l.SegmentId == segmentId && l.ZoneId == null)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Location> CreateAsync(Location location, CancellationToken ct = default)
    {
        if (location.LocationId == Guid.Empty)
        {
            location.LocationId = Guid.NewGuid();
        }

        if (location.CreatedAt == default)
        {
            location.CreatedAt = DateTime.UtcNow;
        }

        location.UpdatedAt = DateTime.UtcNow;

        await _context.MapLocations.AddAsync(location, ct);
        await _context.SaveChangesAsync(ct);

        return location;
    }

    public async Task<Location?> UpdateAsync(Location location, CancellationToken ct = default)
    {
        var existing = await _context.MapLocations
            .FirstOrDefaultAsync(l => l.LocationId == location.LocationId, ct);

        if (existing == null)
        {
            return null;
        }

        location.UpdatedAt = DateTime.UtcNow;

        _context.Entry(existing).CurrentValues.SetValues(location);
        await _context.SaveChangesAsync(ct);

        return existing;
    }

    public async Task<bool> UpdateSegmentIdAsync(Guid locationId, Guid newSegmentId, CancellationToken ct = default)
    {
        var location = await _context.MapLocations
            .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);

        if (location == null)
        {
            return false;
        }

        location.SegmentId = newSegmentId;
        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await _context.MapLocations
            .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);

        if (location == null)
        {
            return false;
        }

        _context.MapLocations.Remove(location);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
