using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;

public interface IStoryMapRepository
{
    // ================== MAP ==================
    Task<Map?> GetMapAsync(Guid mapId, CancellationToken ct);

    // ================== SEGMENT (Slides) ==================
    Task<Segment?> GetSegmentAsync(Guid segmentId, CancellationToken ct);
    Task<Segment?> GetSegmentForUpdateAsync(Guid segmentId, CancellationToken ct);
    Task<List<Segment>> GetSegmentsByMapAsync(Guid mapId, CancellationToken ct);
    Task AddSegmentAsync(Segment segment, CancellationToken ct);
    void UpdateSegment(Segment segment);
    void RemoveSegment(Segment segment);

    // ================== SEGMENT ZONE (Zone highlight trên segment) ==================
    Task<SegmentZone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct);
    Task<List<SegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<SegmentZone>> GetSegmentZonesBySegmentsAsync(List<Guid> segmentIds, CancellationToken ct);
    Task AddSegmentZoneAsync(SegmentZone segmentZone, CancellationToken ct);
    void UpdateSegmentZone(SegmentZone segmentZone);
    void RemoveSegmentZone(SegmentZone segmentZone);

    // ================== SEGMENT LAYER (Layer trên segment) ==================
    Task<SegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct);
    Task<List<SegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<SegmentLayer>> GetSegmentLayersBySegmentsAsync(List<Guid> segmentIds, CancellationToken ct);
    Task AddSegmentLayerAsync(SegmentLayer segmentLayer, CancellationToken ct);
    void UpdateSegmentLayer(SegmentLayer segmentLayer);
    void RemoveSegmentLayer(SegmentLayer segmentLayer);

    // ================== ZONE (Master data - Administrative boundaries) ==================
    Task<Zone?> GetZoneAsync(Guid zoneId, CancellationToken ct);
    Task<Zone?> GetZoneByExternalIdAsync(string externalId, CancellationToken ct);
    Task<List<Zone>> GetZonesByParentAsync(Guid? parentZoneId, CancellationToken ct);
    Task<List<Zone>> GetZonesByTypeAsync(string zoneType, CancellationToken ct);
    Task<List<Zone>> SearchZonesAsync(string searchTerm, CancellationToken ct);
    Task AddZoneAsync(Zone zone, CancellationToken ct);
    void UpdateZone(Zone zone);
    void RemoveZone(Zone zone);

    // ================== LOCATION (Điểm đánh dấu) ==================
    Task<Location?> GetLocationAsync(Guid locationId, CancellationToken ct);
    Task<List<Location>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct);
    Task<List<Location>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddLocationAsync(Location location, CancellationToken ct);
    void UpdateLocation(Location location);
    void RemoveLocation(Location location);

    // ================== TIMELINE TRANSITION (Chuyển cảnh) ==================
    Task<TimelineTransition?> GetTimelineTransitionAsync(Guid transitionId, CancellationToken ct);
    Task<TimelineTransition?> GetTransitionBetweenSegmentsAsync(Guid fromSegmentId, Guid toSegmentId, CancellationToken ct);
    Task<List<TimelineTransition>> GetTimelineTransitionsByMapAsync(Guid mapId, CancellationToken ct);
    Task AddTimelineTransitionAsync(TimelineTransition transition, CancellationToken ct);
    void UpdateTimelineTransition(TimelineTransition transition);
    void RemoveTimelineTransition(TimelineTransition transition);

    // ================== ROUTE ANIMATION (Animation route trong segment) ==================
    Task<RouteAnimation?> GetRouteAnimationAsync(Guid routeAnimationId, CancellationToken ct);
    Task<List<RouteAnimation>> GetRouteAnimationsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<RouteAnimation>> GetRouteAnimationsByMapAsync(Guid mapId, CancellationToken ct);
    Task AddRouteAnimationAsync(RouteAnimation routeAnimation, CancellationToken ct);
    void UpdateRouteAnimation(RouteAnimation routeAnimation);
    void RemoveRouteAnimation(RouteAnimation routeAnimation);

    // ================== ANIMATED LAYER (GIF/Video overlay) ==================
    Task<AnimatedLayer?> GetAnimatedLayerAsync(Guid animatedLayerId, CancellationToken ct);
    Task<List<AnimatedLayer>> GetAnimatedLayersByLayerAsync(Guid layerId, CancellationToken ct);
    Task<List<AnimatedLayer>> GetAnimatedLayersBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<AnimatedLayer>> GetAnimatedLayersByMapAsync(Guid mapId, CancellationToken ct);
    Task AddAnimatedLayerAsync(AnimatedLayer animatedLayer, CancellationToken ct);
    void UpdateAnimatedLayer(AnimatedLayer animatedLayer);
    void RemoveAnimatedLayer(AnimatedLayer animatedLayer);

    // ================== ANIMATED LAYER PRESET (Thư viện GIF) ==================
    Task<AnimatedLayerPreset?> GetAnimatedLayerPresetAsync(Guid presetId, CancellationToken ct);
    Task<List<AnimatedLayerPreset>> GetAnimatedLayerPresetsAsync(CancellationToken ct);
    Task<List<AnimatedLayerPreset>> GetAnimatedLayerPresetsByCategoryAsync(string category, CancellationToken ct);
    Task<List<AnimatedLayerPreset>> SearchAnimatedLayerPresetsAsync(string searchTerm, CancellationToken ct);
    Task AddAnimatedLayerPresetAsync(AnimatedLayerPreset preset, CancellationToken ct);
    void UpdateAnimatedLayerPreset(AnimatedLayerPreset preset);
    void RemoveAnimatedLayerPreset(AnimatedLayerPreset preset);

    // ================== COMMON ==================
    Task<int> SaveChangesAsync(CancellationToken ct);
}
