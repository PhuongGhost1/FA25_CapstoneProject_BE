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

    public Task<Segment?> GetSegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegments
            .Include(s => s.EntryAnimationPreset)
            .Include(s => s.ExitAnimationPreset)
            .Include(s => s.DefaultLayerAnimationPreset)
            .FirstOrDefaultAsync(s => s.SegmentId == segmentId, ct);

    public Task<List<Segment>> GetSegmentsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.MapSegments
            .Where(s => s.MapId == mapId)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentAsync(Segment segment, CancellationToken ct) =>
        _context.MapSegments.AddAsync(segment, ct).AsTask();

    public void UpdateSegment(Segment segment) =>
        _context.MapSegments.Update(segment);

    public void RemoveSegment(Segment segment) =>
        _context.MapSegments.Remove(segment);

    public Task<SegmentZone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct) =>
        _context.MapSegmentZones.FirstOrDefaultAsync(z => z.SegmentZoneId == segmentZoneId, ct);

    public Task<List<SegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegmentZones
            .Where(z => z.SegmentId == segmentId)
            .OrderBy(z => z.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentZoneAsync(SegmentZone zone, CancellationToken ct) =>
        _context.MapSegmentZones.AddAsync(zone, ct).AsTask();

    public void UpdateSegmentZone(SegmentZone zone) =>
        _context.MapSegmentZones.Update(zone);

    public void RemoveSegmentZone(SegmentZone zone) =>
        _context.MapSegmentZones.Remove(zone);

    public Task<Location?> GetLocationAsync(Guid locationId, CancellationToken ct) =>
        _context.MapLocations.FirstOrDefaultAsync(l => l.LocationId == locationId, ct);

    public Task<List<Location>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.MapLocations
            .Where(l => l.MapId == mapId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<Location>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapLocations
            .Where(l => l.SegmentId == segmentId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task AddLocationAsync(Location location, CancellationToken ct) =>
        _context.MapLocations.AddAsync(location, ct).AsTask();

    public void UpdateLocation(Location location) =>
        _context.MapLocations.Update(location);

    public void RemoveLocation(Location location) =>
        _context.MapLocations.Remove(location);

    public Task<SegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct) =>
        _context.MapSegmentLayers.FirstOrDefaultAsync(l => l.SegmentLayerId == segmentLayerId, ct);

    public Task<List<SegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.MapSegmentLayers
            .Where(l => l.SegmentId == segmentId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentLayerAsync(SegmentLayer layer, CancellationToken ct) =>
        _context.MapSegmentLayers.AddAsync(layer, ct).AsTask();

    public void UpdateSegmentLayer(SegmentLayer layer) =>
        _context.MapSegmentLayers.Update(layer);

    public void RemoveSegmentLayer(SegmentLayer layer) =>
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

    public Task<SegmentTransition?> GetSegmentTransitionAsync(Guid transitionId, CancellationToken ct) =>
        _context.SegmentTransitions
            .Include(t => t.FromSegment)
            .Include(t => t.ToSegment)
            .Include(t => t.AnimationPreset)
            .FirstOrDefaultAsync(t => t.SegmentTransitionId == transitionId, ct);

    public Task<List<SegmentTransition>> GetSegmentTransitionsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.SegmentTransitions
            .Include(t => t.FromSegment)
            .Include(t => t.ToSegment)
            .Include(t => t.AnimationPreset)
            .Where(t => t.FromSegment!.MapId == mapId || t.ToSegment!.MapId == mapId)
            .ToListAsync(ct);

    public Task AddSegmentTransitionAsync(SegmentTransition transition, CancellationToken ct) =>
        _context.SegmentTransitions.AddAsync(transition, ct).AsTask();

    public void UpdateSegmentTransition(SegmentTransition transition) =>
        _context.SegmentTransitions.Update(transition);

    public void RemoveSegmentTransition(SegmentTransition transition) =>
        _context.SegmentTransitions.Remove(transition);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
