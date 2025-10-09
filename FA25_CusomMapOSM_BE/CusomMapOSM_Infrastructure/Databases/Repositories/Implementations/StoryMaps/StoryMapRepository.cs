using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.StoryMaps;

public class StoryMapRepository : IStoryMapRepository
{
    private readonly CustomMapOSMDbContext _context;

    public StoryMapRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public Task<Map?> GetMapAsync(Guid mapId, CancellationToken ct) =>
        _context.Maps.FirstOrDefaultAsync(m => m.MapId == mapId, ct);

    public Task<MapSegment?> GetSegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegments
            .Include(s => s.EntryAnimationPreset)
            .Include(s => s.ExitAnimationPreset)
            .Include(s => s.DefaultLayerAnimationPreset)
            .FirstOrDefaultAsync(s => s.SegmentId == segmentId, ct);

    public Task<List<MapSegment>> GetSegmentsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.MapSegments
            .Where(s => s.MapId == mapId)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentAsync(MapSegment segment, CancellationToken ct) =>
        _context.MapSegments.AddAsync(segment, ct).AsTask();

    public void UpdateSegment(MapSegment segment) =>
        _context.MapSegments.Update(segment);

    public void RemoveSegment(MapSegment segment) =>
        _context.MapSegments.Remove(segment);

    public Task<MapSegmentZone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct) =>
        _context.MapSegmentZones.FirstOrDefaultAsync(z => z.SegmentZoneId == segmentZoneId, ct);

    public Task<List<MapSegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegmentZones
            .Where(z => z.SegmentId == segmentId)
            .OrderBy(z => z.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentZoneAsync(MapSegmentZone zone, CancellationToken ct) =>
        _context.MapSegmentZones.AddAsync(zone, ct).AsTask();

    public void UpdateSegmentZone(MapSegmentZone zone) =>
        _context.MapSegmentZones.Update(zone);

    public void RemoveSegmentZone(MapSegmentZone zone) =>
        _context.MapSegmentZones.Remove(zone);

    public Task<MapLocation?> GetLocationAsync(Guid locationId, CancellationToken ct) =>
        _context.MapLocations.FirstOrDefaultAsync(l => l.MapLocationId == locationId, ct);

    public Task<List<MapLocation>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.MapLocations
            .Where(l => l.MapId == mapId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<MapLocation>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapLocations
            .Where(l => l.SegmentId == segmentId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task AddLocationAsync(MapLocation location, CancellationToken ct) =>
        _context.MapLocations.AddAsync(location, ct).AsTask();

    public void UpdateLocation(MapLocation location) =>
        _context.MapLocations.Update(location);

    public void RemoveLocation(MapLocation location) =>
        _context.MapLocations.Remove(location);

    public Task<MapSegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct) =>
        _context.MapSegmentLayers.FirstOrDefaultAsync(l => l.SegmentLayerId == segmentLayerId, ct);

    public Task<List<MapSegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegmentLayers
            .Where(l => l.SegmentId == segmentId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentLayerAsync(MapSegmentLayer layer, CancellationToken ct) =>
        _context.MapSegmentLayers.AddAsync(layer, ct).AsTask();

    public void UpdateSegmentLayer(MapSegmentLayer layer) =>
        _context.MapSegmentLayers.Update(layer);

    public void RemoveSegmentLayer(MapSegmentLayer layer) =>
        _context.MapSegmentLayers.Remove(layer);

    public Task<TimelineStep?> GetTimelineStepAsync(Guid timelineStepId, CancellationToken ct) =>
        _context.TimelineSteps.FirstOrDefaultAsync(t => t.TimelineStepId == timelineStepId, ct);

    public Task<List<TimelineStep>> GetTimelineByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.TimelineSteps
            .Where(t => t.MapId == mapId)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(ct);

    public Task AddTimelineStepAsync(TimelineStep step, CancellationToken ct) =>
        _context.TimelineSteps.AddAsync(step, ct).AsTask();

    public void UpdateTimelineStep(TimelineStep step) =>
        _context.TimelineSteps.Update(step);

    public void RemoveTimelineStep(TimelineStep step) =>
        _context.TimelineSteps.Remove(step);

    public Task<List<TimelineStepLayer>> GetTimelineStepLayersAsync(Guid timelineStepId, CancellationToken ct) =>
        _context.TimelineStepLayers
            .Where(l => l.TimelineStepId == timelineStepId)
            .OrderBy(l => l.DelayMs)
            .ToListAsync(ct);

    public Task AddTimelineStepLayerAsync(TimelineStepLayer layer, CancellationToken ct) =>
        _context.TimelineStepLayers.AddAsync(layer, ct).AsTask();

    public void AddTimelineStepLayers(IEnumerable<TimelineStepLayer> layers) =>
        _context.TimelineStepLayers.AddRange(layers);

    public void RemoveTimelineStepLayers(IEnumerable<TimelineStepLayer> layers) =>
        _context.TimelineStepLayers.RemoveRange(layers);

    public Task<List<AdministrativeZone>> GetAdministrativeZonesAsync(IEnumerable<Guid> zoneIds, CancellationToken ct) =>
        _context.AdministrativeZones
            .Where(z => zoneIds.Contains(z.ZoneId))
            .ToListAsync(ct);

    public Task<List<ZoneStatistic>> GetZoneStatisticsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct) =>
        _context.ZoneStatistics
            .Where(s => zoneIds.Contains(s.ZoneId))
            .ToListAsync(ct);

    public Task<List<ZoneInsight>> GetZoneInsightsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct) =>
        _context.ZoneInsights
            .Where(i => zoneIds.Contains(i.ZoneId))
            .ToListAsync(ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
