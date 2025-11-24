using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.StoryMaps;

public interface IStoryMapService
{
    // ================== SEGMENT (Slides) ==================
    Task<Option<IReadOnlyCollection<SegmentDto>, Error>> GetSegmentsAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> GetSegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> UpdateSegmentAsync(Guid segmentId, UpdateSegmentRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> DuplicateSegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<bool, Error>> ReorderSegmentsAsync(Guid mapId, List<Guid> segmentIds, CancellationToken ct = default);

    // ================== SEGMENT ZONE (Zone highlight trên segment) ==================
    Task<Option<IReadOnlyCollection<SegmentZoneDto>, Error>> GetSegmentZonesAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentZoneDto, Error>> CreateSegmentZoneAsync(CreateSegmentZoneV2Request request, CancellationToken ct = default);
    Task<Option<SegmentZoneDto, Error>> UpdateSegmentZoneAsync(Guid segmentZoneId, UpdateSegmentZoneV2Request request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct = default);
    Task<Option<bool, Error>> MoveZoneToSegmentAsync(Guid segmentZoneId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default);

    // ================== SEGMENT LAYER (Layer trên segment) ==================
    Task<Option<IReadOnlyCollection<SegmentLayerDto>, Error>> GetSegmentLayersAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentLayerDto, Error>> CreateSegmentLayerAsync(CreateSegmentLayerRequest request, CancellationToken ct = default);
    Task<Option<SegmentLayerDto, Error>> UpdateSegmentLayerAsync(Guid segmentLayerId, UpdateSegmentLayerRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct = default);
    Task<Option<bool, Error>> MoveLayerToSegmentAsync(Guid segmentLayerId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default);

    // ================== ZONE (Master data - Administrative boundaries) ==================
    Task<Option<IReadOnlyCollection<ZoneDto>, Error>> GetZonesAsync(CancellationToken ct = default);
    Task<Option<ZoneDto, Error>> GetZoneAsync(Guid zoneId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<ZoneDto>, Error>> GetZonesByParentAsync(Guid? parentZoneId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<ZoneDto>, Error>> SearchZonesAsync(string searchTerm, CancellationToken ct = default);
    Task<Option<ZoneDto, Error>> CreateZoneAsync(CreateZoneRequest request, CancellationToken ct = default);
    Task<Option<ZoneDto, Error>> CreateZoneFromOsmAsync(CreateZoneFromOsmRequest request, CancellationToken ct = default);
    Task<Option<ZoneDto, Error>> UpdateZoneAsync(Guid zoneId, UpdateZoneRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteZoneAsync(Guid zoneId, CancellationToken ct = default);
    Task<Option<int, Error>> SyncZonesFromOSMAsync(SyncZonesFromOSMRequest request, CancellationToken ct = default);

    // ================== TIMELINE TRANSITION (Chuyển cảnh giữa các segments) ==================
    Task<Option<IReadOnlyCollection<TimelineTransitionDto>, Error>> GetTimelineTransitionsAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<TimelineTransitionDto, Error>> GetTimelineTransitionAsync(Guid transitionId, CancellationToken ct = default);
    Task<Option<TimelineTransitionDto, Error>> CreateTimelineTransitionAsync(CreateTimelineTransitionRequest request, CancellationToken ct = default);
    Task<Option<TimelineTransitionDto, Error>> UpdateTimelineTransitionAsync(Guid transitionId, UpdateTimelineTransitionRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteTimelineTransitionAsync(Guid transitionId, CancellationToken ct = default);
    Task<Option<TimelineTransitionDto, Error>> GenerateTimelineTransitionAsync(Guid mapId, GenerateTimelineTransitionRequest request, CancellationToken ct = default);

    // ================== ROUTE ANIMATION (Animation route trong segment) ==================
    Task<Option<IReadOnlyCollection<RouteAnimationDto>, Error>> GetRouteAnimationsBySegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<RouteAnimationDto, Error>> GetRouteAnimationAsync(Guid routeAnimationId, CancellationToken ct = default);
    Task<Option<RouteAnimationDto, Error>> CreateRouteAnimationAsync(CreateRouteAnimationRequest request, CancellationToken ct = default);
    Task<Option<RouteAnimationDto, Error>> UpdateRouteAnimationAsync(Guid routeAnimationId, UpdateRouteAnimationRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteRouteAnimationAsync(Guid routeAnimationId, CancellationToken ct = default);
    Task<Option<bool, Error>> MoveRouteToSegmentAsync(Guid routeAnimationId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default);

    // ================== ANIMATED LAYER (GIF/Video overlay trên segment) ==================
    Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersBySegmentAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersByLayerAsync(Guid layerId, CancellationToken ct = default);
    Task<Option<AnimatedLayerDto, Error>> GetAnimatedLayerAsync(Guid animatedLayerId, CancellationToken ct = default);
    Task<Option<AnimatedLayerDto, Error>> CreateAnimatedLayerAsync(CreateAnimatedLayerRequest request, Guid userId, CancellationToken ct = default);
    Task<Option<AnimatedLayerDto, Error>> UpdateAnimatedLayerAsync(Guid animatedLayerId, UpdateAnimatedLayerRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteAnimatedLayerAsync(Guid animatedLayerId, CancellationToken ct = default);

    // ================== ANIMATED LAYER PRESET ==================
    Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>> GetAnimatedLayerPresetsAsync(CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>> GetAnimatedLayerPresetsByCategoryAsync(string category, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>> SearchAnimatedLayerPresetsAsync(string searchTerm, CancellationToken ct = default);
    Task<Option<AnimatedLayerPresetDto, Error>> GetAnimatedLayerPresetAsync(Guid presetId, CancellationToken ct = default);
    Task<Option<AnimatedLayerPresetDto, Error>> CreateAnimatedLayerPresetAsync(CreateAnimatedLayerPresetRequest request, Guid? userId, CancellationToken ct = default);
    Task<Option<AnimatedLayerPresetDto, Error>> UpdateAnimatedLayerPresetAsync(Guid presetId, UpdateAnimatedLayerPresetRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteAnimatedLayerPresetAsync(Guid presetId, CancellationToken ct = default);
    Task<Option<AnimatedLayerDto, Error>> CreateAnimatedLayerFromPresetAsync(Guid presetId, Guid? layerId, Guid? segmentId, CancellationToken ct = default);

    // ================== SEARCH ==================
    Task<Option<IReadOnlyCollection<LocationDto>, Error>> SearchLocationsAsync(string searchTerm, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<ZoneDto>, Error>> SearchRoutesAsync(string from, string to, CancellationToken ct = default);
    Task<Option<string, Error>> SearchRouteBetweenLocationsAsync(Guid fromLocationId, Guid toLocationId, string routeType = "road", CancellationToken ct = default);
    Task<Option<string, Error>> SearchRouteWithMultipleLocationsAsync(List<Guid> locationIds, string routeType = "road", CancellationToken ct = default);
}
