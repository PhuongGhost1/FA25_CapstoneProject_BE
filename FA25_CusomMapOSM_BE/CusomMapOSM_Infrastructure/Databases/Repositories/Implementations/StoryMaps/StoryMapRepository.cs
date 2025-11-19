using CusomMapOSM_Domain.Entities.Animations;
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
        _context.SegmentZones
            .Include(sz => sz.Zone)
            .Include(sz => sz.Segment)
            .FirstOrDefaultAsync(sz => sz.SegmentZoneId == segmentZoneId, ct);

    public Task<List<SegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.SegmentZones
            .Include(sz => sz.Zone)
            .Where(sz => sz.SegmentId == segmentId)
            .OrderBy(sz => sz.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentZoneAsync(SegmentZone segmentZone, CancellationToken ct) =>
        _context.SegmentZones.AddAsync(segmentZone, ct).AsTask();

    public void UpdateSegmentZone(SegmentZone segmentZone) =>
        _context.SegmentZones.Update(segmentZone);

    public void RemoveSegmentZone(SegmentZone segmentZone) =>
        _context.SegmentZones.Remove(segmentZone);

    public Task<SegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct) =>
        _context.SegmentLayers
            .Include(sl => sl.Layer)
            .Include(sl => sl.Segment)
            .FirstOrDefaultAsync(sl => sl.SegmentLayerId == segmentLayerId, ct);

    public Task<List<SegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.SegmentLayers
            .Include(sl => sl.Layer)
            .Where(sl => sl.SegmentId == segmentId)
            .OrderBy(sl => sl.DisplayOrder)
            .ToListAsync(ct);

    public Task AddSegmentLayerAsync(SegmentLayer segmentLayer, CancellationToken ct) =>
        _context.SegmentLayers.AddAsync(segmentLayer, ct).AsTask();

    public void UpdateSegmentLayer(SegmentLayer segmentLayer) =>
        _context.SegmentLayers.Update(segmentLayer);

    public void RemoveSegmentLayer(SegmentLayer segmentLayer) =>
        _context.SegmentLayers.Remove(segmentLayer);

    public Task<Zone?> GetZoneAsync(Guid zoneId, CancellationToken ct) =>
        _context.Zones
            .Include(z => z.ParentZone)
            .FirstOrDefaultAsync(z => z.ZoneId == zoneId, ct);

    public Task<Zone?> GetZoneByExternalIdAsync(string externalId, CancellationToken ct) =>
        _context.Zones
            .FirstOrDefaultAsync(z => z.ExternalId == externalId, ct);

    public Task<List<Zone>> GetZonesByParentAsync(Guid? parentZoneId, CancellationToken ct) =>
        _context.Zones
            .Where(z => z.ParentZoneId == parentZoneId && z.IsActive)
            .OrderBy(z => z.Name)
            .ToListAsync(ct);

    public async Task<List<Zone>> GetZonesByTypeAsync(string zoneType, CancellationToken ct)
    {
        // Load all active zones first (to avoid ToString() in LINQ query)
        var allZones = await _context.Zones
            .Where(z => z.IsActive)
            .ToListAsync(ct);
        
        // Filter by zoneType in memory (ZoneType is stored as lowercase string in DB)
        var zoneTypeLower = zoneType?.ToLowerInvariant();
        return allZones
            .Where(z => z.ZoneType.ToString().ToLowerInvariant() == zoneTypeLower)
            .OrderBy(z => z.Name)
            .ToList();
    }

    public Task<List<Zone>> SearchZonesAsync(string searchTerm, CancellationToken ct) =>
        _context.Zones
            .Where(z => z.IsActive && (
                z.Name.Contains(searchTerm) ||
                z.ZoneCode.Contains(searchTerm) ||
                z.ExternalId.Contains(searchTerm)
            ))
            .OrderBy(z => z.Name)
            .Take(50)
            .ToListAsync(ct);

    public Task AddZoneAsync(Zone zone, CancellationToken ct) =>
        _context.Zones.AddAsync(zone, ct).AsTask();

    public void UpdateZone(Zone zone) =>
        _context.Zones.Update(zone);

    public void RemoveZone(Zone zone) =>
        _context.Zones.Remove(zone);

    public Task<Location?> GetLocationAsync(Guid locationId, CancellationToken ct) =>
        _context.MapLocations
            .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);

    public Task<List<Location>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.MapLocations
            .Where(l => l.Segment != null && l.Segment.MapId == mapId)
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

    public Task<TimelineTransition?> GetTimelineTransitionAsync(Guid transitionId, CancellationToken ct) =>
        _context.TimelineTransitions
            .Include(t => t.FromSegment)
            .Include(t => t.ToSegment)
            .FirstOrDefaultAsync(t => t.TimelineTransitionId == transitionId, ct);

    public Task<TimelineTransition?> GetTransitionBetweenSegmentsAsync(Guid fromSegmentId, Guid toSegmentId, CancellationToken ct) =>
        _context.TimelineTransitions
            .FirstOrDefaultAsync(t => t.FromSegmentId == fromSegmentId && t.ToSegmentId == toSegmentId, ct);

    public Task<List<TimelineTransition>> GetTimelineTransitionsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.TimelineTransitions
            .Include(t => t.FromSegment)
            .Include(t => t.ToSegment)
            .Where(t => t.MapId == mapId)
            .ToListAsync(ct);

    public Task AddTimelineTransitionAsync(TimelineTransition transition, CancellationToken ct) =>
        _context.TimelineTransitions.AddAsync(transition, ct).AsTask();

    public void UpdateTimelineTransition(TimelineTransition transition) =>
        _context.TimelineTransitions.Update(transition);

    public void RemoveTimelineTransition(TimelineTransition transition) =>
        _context.TimelineTransitions.Remove(transition);

    public Task<RouteAnimation?> GetRouteAnimationAsync(Guid routeAnimationId, CancellationToken ct) =>
        _context.RouteAnimations
            .Include(ra => ra.Segment)
            .FirstOrDefaultAsync(ra => ra.RouteAnimationId == routeAnimationId, ct);

    public Task<List<RouteAnimation>> GetRouteAnimationsBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.RouteAnimations
            .Where(ra => ra.SegmentId == segmentId)
            .OrderBy(ra => ra.DisplayOrder)
            .ThenBy(ra => ra.CreatedAt)
            .ToListAsync(ct);

    public Task<List<RouteAnimation>> GetRouteAnimationsByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.RouteAnimations
            .Where(ra => ra.MapId == mapId)
            .OrderBy(ra => ra.DisplayOrder)
            .ThenBy(ra => ra.CreatedAt)
            .ToListAsync(ct);

    public Task AddRouteAnimationAsync(RouteAnimation routeAnimation, CancellationToken ct) =>
        _context.RouteAnimations.AddAsync(routeAnimation, ct).AsTask();

    public void UpdateRouteAnimation(RouteAnimation routeAnimation) =>
        _context.RouteAnimations.Update(routeAnimation);

    public void RemoveRouteAnimation(RouteAnimation routeAnimation) =>
        _context.RouteAnimations.Remove(routeAnimation);

    public Task<AnimatedLayer?> GetAnimatedLayerAsync(Guid animatedLayerId, CancellationToken ct) =>
        _context.AnimatedLayers
            .Include(al => al.Layer)
            .Include(al => al.Segment)
            .FirstOrDefaultAsync(al => al.AnimatedLayerId == animatedLayerId, ct);

    public Task<List<AnimatedLayer>> GetAnimatedLayersByLayerAsync(Guid layerId, CancellationToken ct) =>
        _context.AnimatedLayers
            .Where(al => al.LayerId == layerId && al.IsVisible)
            .OrderBy(al => al.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<AnimatedLayer>> GetAnimatedLayersBySegmentAsync(Guid segmentId, CancellationToken ct) =>
        _context.AnimatedLayers
            .Where(al => al.SegmentId == segmentId && al.IsVisible)
            .OrderBy(al => al.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<AnimatedLayer>> GetAnimatedLayersByMapAsync(Guid mapId, CancellationToken ct) =>
        _context.AnimatedLayers
            .Include(al => al.Segment)
            .Where(al => al.Segment != null && al.Segment.MapId == mapId && al.IsVisible)
            .OrderBy(al => al.DisplayOrder)
            .ToListAsync(ct);

    public Task AddAnimatedLayerAsync(AnimatedLayer animatedLayer, CancellationToken ct) =>
        _context.AnimatedLayers.AddAsync(animatedLayer, ct).AsTask();

    public void UpdateAnimatedLayer(AnimatedLayer animatedLayer) =>
        _context.AnimatedLayers.Update(animatedLayer);

    public void RemoveAnimatedLayer(AnimatedLayer animatedLayer) =>
        _context.AnimatedLayers.Remove(animatedLayer);

    public Task<AnimatedLayerPreset?> GetAnimatedLayerPresetAsync(Guid presetId, CancellationToken ct) =>
        _context.AnimatedLayerPresets
            .FirstOrDefaultAsync(p => p.AnimatedLayerPresetId == presetId && p.IsActive, ct);

    public Task<List<AnimatedLayerPreset>> GetAnimatedLayerPresetsAsync(CancellationToken ct) =>
        _context.AnimatedLayerPresets
            .Where(p => p.IsActive && p.IsPublic)
            .OrderByDescending(p => p.UsageCount)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

    public Task<List<AnimatedLayerPreset>> GetAnimatedLayerPresetsByCategoryAsync(string category, CancellationToken ct) =>
        _context.AnimatedLayerPresets
            .Where(p => p.IsActive && p.IsPublic && p.Category == category)
            .OrderByDescending(p => p.UsageCount)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

    public Task<List<AnimatedLayerPreset>> SearchAnimatedLayerPresetsAsync(string searchTerm, CancellationToken ct) =>
        _context.AnimatedLayerPresets
            .Where(p => p.IsActive && p.IsPublic && (
                p.Name.Contains(searchTerm) ||
                (p.Tags != null && p.Tags.Contains(searchTerm)) ||
                (p.Description != null && p.Description.Contains(searchTerm))
            ))
            .OrderByDescending(p => p.UsageCount)
            .Take(20)
            .ToListAsync(ct);

    public Task AddAnimatedLayerPresetAsync(AnimatedLayerPreset preset, CancellationToken ct) =>
        _context.AnimatedLayerPresets.AddAsync(preset, ct).AsTask();

    public void UpdateAnimatedLayerPreset(AnimatedLayerPreset preset) =>
        _context.AnimatedLayerPresets.Update(preset);

    public void RemoveAnimatedLayerPreset(AnimatedLayerPreset preset) =>
        _context.AnimatedLayerPresets.Remove(preset);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
