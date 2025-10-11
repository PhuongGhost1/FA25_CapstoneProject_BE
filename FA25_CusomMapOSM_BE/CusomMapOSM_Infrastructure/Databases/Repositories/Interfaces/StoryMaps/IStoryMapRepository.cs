using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;

public interface IStoryMapRepository
{
    Task<Map?> GetMapAsync(Guid mapId, CancellationToken ct);

    Task<MapSegment?> GetSegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<MapSegment>> GetSegmentsByMapAsync(Guid mapId, CancellationToken ct);
    Task AddSegmentAsync(MapSegment segment, CancellationToken ct);
    void UpdateSegment(MapSegment segment);
    void RemoveSegment(MapSegment segment);

    Task<MapSegmentZone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct);
    Task<List<MapSegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddSegmentZoneAsync(MapSegmentZone zone, CancellationToken ct);
    void UpdateSegmentZone(MapSegmentZone zone);
    void RemoveSegmentZone(MapSegmentZone zone);

    Task<MapLocation?> GetLocationAsync(Guid locationId, CancellationToken ct);
    Task<List<MapLocation>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct);
    Task<List<MapLocation>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddLocationAsync(MapLocation location, CancellationToken ct);
    void UpdateLocation(MapLocation location);
    void RemoveLocation(MapLocation location);

    Task<MapSegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct);
    Task<List<MapSegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddSegmentLayerAsync(MapSegmentLayer layer, CancellationToken ct);
    void UpdateSegmentLayer(MapSegmentLayer layer);
    void RemoveSegmentLayer(MapSegmentLayer layer);

    Task<TimelineStep?> GetTimelineStepAsync(Guid timelineStepId, CancellationToken ct);
    Task<List<TimelineStep>> GetTimelineByMapAsync(Guid mapId, CancellationToken ct);
    Task AddTimelineStepAsync(TimelineStep step, CancellationToken ct);
    void UpdateTimelineStep(TimelineStep step);
    void RemoveTimelineStep(TimelineStep step);
    Task<List<TimelineStepLayer>> GetTimelineStepLayersAsync(Guid timelineStepId, CancellationToken ct);
    Task AddTimelineStepLayerAsync(TimelineStepLayer layer, CancellationToken ct);
    void AddTimelineStepLayers(IEnumerable<TimelineStepLayer> layers);
    void RemoveTimelineStepLayers(IEnumerable<TimelineStepLayer> layers);

    Task<List<AdministrativeZone>> GetAdministrativeZonesAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);
    Task<List<ZoneStatistic>> GetZoneStatisticsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);
    Task<List<ZoneInsight>> GetZoneInsightsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);

    Task<int> SaveChangesAsync(CancellationToken ct);
}
