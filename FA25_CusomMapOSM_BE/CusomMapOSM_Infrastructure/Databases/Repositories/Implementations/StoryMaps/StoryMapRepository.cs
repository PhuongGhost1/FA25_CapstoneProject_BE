using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.StoryElement;
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

    public Task<Zone?> GetSegmentZoneAsync(Guid zoneId, CancellationToken ct) =>
        _context.Zones.FirstOrDefaultAsync(z => z.ZoneId == zoneId, ct);

    public Task<List<Zone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.Zones
            .Where(z => z.SegmentId == segmentId)
            .OrderBy(z => z.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentZoneAsync(Zone zone, CancellationToken ct) =>
        _context.Zones.AddAsync(zone, ct).AsTask();

    public void UpdateSegmentZone(Zone zone) =>
        _context.Zones.Update(zone);

    public void RemoveSegmentZone(Zone zone) =>
        _context.Zones.Remove(zone);

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


    public Task<TimelineStep?> GetTimelineStepAsync(Guid timelineStepId, CancellationToken ct) =>
        _context.TimelineSteps.FirstOrDefaultAsync(t => t.TimelineStepId == timelineStepId, ct);

    public Task<List<TimelineStep>> GetTimelineByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.TimelineSteps
            .Where(t => t.MapId == mapId)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<TimelineStep>> GetTimelineStepsBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.TimelineSteps
            .Where(t => t.SegmentId == segmentId)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(ct);

    public Task AddTimelineStepAsync(TimelineStep step, CancellationToken ct) =>
        _context.TimelineSteps.AddAsync(step, ct).AsTask();

    public void UpdateTimelineStep(TimelineStep step) =>
        _context.TimelineSteps.Update(step);

    public void RemoveTimelineStep(TimelineStep step) =>
        _context.TimelineSteps.Remove(step);
    
    public Task<StoryElementLayer?> GetStoryElementLayerAsync(Guid storyElementLayerId, CancellationToken ct) =>
        _context.StoryElementLayers
            .Include(sel => sel.Layer)
            .Include(sel => sel.Zone)
            .Include(sel => sel.AnimationPreset)
            .FirstOrDefaultAsync(sel => sel.StoryElementLayerId == storyElementLayerId, ct);

    public Task<List<StoryElementLayer>> GetStoryElementLayersByElementAsync(Guid elementId, CancellationToken ct) =>
        _context.StoryElementLayers
            .Include(sel => sel.Layer)
            .Include(sel => sel.Zone)
            .Include(sel => sel.AnimationPreset)
            .Where(sel => sel.ElementId == elementId)
            .OrderBy(sel => sel.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<StoryElementLayer>> GetStoryElementLayersByLayerAsync(Guid layerId, CancellationToken ct) =>
        _context.StoryElementLayers
            .Include(sel => sel.Layer)
            .Include(sel => sel.Zone)
            .Include(sel => sel.AnimationPreset)
            .Where(sel => sel.LayerId == layerId)
            .OrderBy(sel => sel.DisplayOrder)
            .ToListAsync(ct);

    public Task AddStoryElementLayerAsync(StoryElementLayer storyElementLayer, CancellationToken ct) =>
        _context.StoryElementLayers.AddAsync(storyElementLayer, ct).AsTask();

    public void UpdateStoryElementLayer(StoryElementLayer storyElementLayer) =>
        _context.StoryElementLayers.Update(storyElementLayer);

    public void RemoveStoryElementLayer(StoryElementLayer storyElementLayer) =>
        _context.StoryElementLayers.Remove(storyElementLayer);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
