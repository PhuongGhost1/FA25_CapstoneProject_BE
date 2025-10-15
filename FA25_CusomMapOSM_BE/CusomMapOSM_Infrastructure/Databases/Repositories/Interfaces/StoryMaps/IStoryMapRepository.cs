using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;

public interface IStoryMapRepository
{
    Task<Map?> GetMapAsync(Guid mapId, CancellationToken ct);

    Task<Segment?> GetSegmentAsync(Guid segmentId, CancellationToken ct);
    Task<List<Segment>> GetSegmentsByMapAsync(Guid mapId, CancellationToken ct);
    Task AddSegmentAsync(Segment segment, CancellationToken ct);
    void UpdateSegment(Segment segment);
    void RemoveSegment(Segment segment);

    Task<SegmentZone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct);
    Task<List<SegmentZone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddSegmentZoneAsync(SegmentZone zone, CancellationToken ct);
    void UpdateSegmentZone(SegmentZone zone);
    void RemoveSegmentZone(SegmentZone zone);

    Task<Location?> GetLocationAsync(Guid locationId, CancellationToken ct);
    Task<List<Location>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct);
    Task<List<Location>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddLocationAsync(Location location, CancellationToken ct);
    void UpdateLocation(Location location);
    void RemoveLocation(Location location);

    Task<SegmentLayer?> GetSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct);
    Task<List<SegmentLayer>> GetSegmentLayersBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddSegmentLayerAsync(SegmentLayer layer, CancellationToken ct);
    void UpdateSegmentLayer(SegmentLayer layer);
    void RemoveSegmentLayer(SegmentLayer layer);

    Task<TimelineStep?> GetTimelineStepAsync(Guid timelineStepId, CancellationToken ct);
    Task<List<TimelineStep>> GetTimelineByMapAsync(Guid mapId, CancellationToken ct);
    Task AddTimelineStepAsync(TimelineStep step, CancellationToken ct);
    void UpdateTimelineStep(TimelineStep step);
    void RemoveTimelineStep(TimelineStep step);
    Task<List<TimelineStepLayer>> GetTimelineStepLayersAsync(Guid timelineStepId, CancellationToken ct);
    Task AddTimelineStepLayerAsync(TimelineStepLayer layer, CancellationToken ct);
    void AddTimelineStepLayers(IEnumerable<TimelineStepLayer> layers);
    void RemoveTimelineStepLayers(IEnumerable<TimelineStepLayer> layers);

    Task<List<Zone>> GetAdministrativeZonesAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);
    Task<List<ZoneStatistic>> GetZoneStatisticsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);
    Task<List<ZoneInsight>> GetZoneInsightsAsync(IEnumerable<Guid> zoneIds, CancellationToken ct);

    Task<int> SaveChangesAsync(CancellationToken ct);
}
