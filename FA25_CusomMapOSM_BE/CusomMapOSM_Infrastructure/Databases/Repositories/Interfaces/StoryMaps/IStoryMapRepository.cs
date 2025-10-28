using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.StoryElement;
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

    Task<Zone?> GetSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct);
    Task<List<Zone>> GetSegmentZonesBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddSegmentZoneAsync(Zone zone, CancellationToken ct);
    void UpdateSegmentZone(Zone zone);
    void RemoveSegmentZone(Zone zone);

    Task<Location?> GetLocationAsync(Guid locationId, CancellationToken ct);
    Task<List<Location>> GetLocationsByMapAsync(Guid mapId, CancellationToken ct);
    Task<List<Location>> GetLocationsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddLocationAsync(Location location, CancellationToken ct);
    void UpdateLocation(Location location);
    void RemoveLocation(Location location);


    Task<TimelineStep?> GetTimelineStepAsync(Guid timelineStepId, CancellationToken ct);
    Task<List<TimelineStep>> GetTimelineByMapAsync(Guid mapId, CancellationToken ct);
    Task<List<TimelineStep>> GetTimelineStepsBySegmentAsync(Guid segmentId, CancellationToken ct);
    Task AddTimelineStepAsync(TimelineStep step, CancellationToken ct);
    void UpdateTimelineStep(TimelineStep step);
    void RemoveTimelineStep(TimelineStep step);
    
    Task<StoryElementLayer?> GetStoryElementLayerAsync(Guid storyElementLayerId, CancellationToken ct);
    Task<List<StoryElementLayer>> GetStoryElementLayersByElementAsync(Guid elementId, CancellationToken ct);
    Task<List<StoryElementLayer>> GetStoryElementLayersByLayerAsync(Guid layerId, CancellationToken ct);
    Task AddStoryElementLayerAsync(StoryElementLayer storyElementLayer, CancellationToken ct);
    void UpdateStoryElementLayer(StoryElementLayer storyElementLayer);
    void RemoveStoryElementLayer(StoryElementLayer storyElementLayer);

    Task<int> SaveChangesAsync(CancellationToken ct);
}
