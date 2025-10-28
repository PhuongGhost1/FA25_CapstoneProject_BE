using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.StoryMaps;

public interface IStoryMapService
{
    Task<Option<IReadOnlyCollection<SegmentDto>, Error>> GetSegmentsAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default);
    Task<Option<SegmentDto, Error>> UpdateSegmentAsync(Guid segmentId, UpdateSegmentRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentAsync(Guid segmentId, CancellationToken ct = default);

    Task<Option<IReadOnlyCollection<SegmentZoneDto>, Error>> GetSegmentZonesAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentZoneDto, Error>> CreateSegmentZoneAsync(CreateSegmentZoneRequest request, CancellationToken ct = default);
    Task<Option<SegmentZoneDto, Error>> UpdateSegmentZoneAsync(Guid segmentZoneId, UpdateSegmentZoneRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct = default);

    Task<Option<IReadOnlyCollection<SegmentLayerDto>, Error>> GetSegmentLayersAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<SegmentLayerDto, Error>> CreateSegmentLayerAsync(Guid segmentId, UpsertSegmentLayerRequest request, CancellationToken ct = default);
    Task<Option<SegmentLayerDto, Error>> UpdateSegmentLayerAsync(Guid segmentLayerId, UpsertSegmentLayerRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct = default);

    Task<Option<IReadOnlyCollection<TimelineStepDto>, Error>> GetTimelineAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<TimelineStepDto, Error>> CreateTimelineStepAsync(CreateTimelineStepRequest request, CancellationToken ct = default);
    Task<Option<TimelineStepDto, Error>> UpdateTimelineStepAsync(Guid timelineStepId, UpdateTimelineStepRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteTimelineStepAsync(Guid timelineStepId, CancellationToken ct = default);

    Task<Option<TransitionPreviewDto, Error>> PreviewTransitionAsync(PreviewTransitionRequest request, CancellationToken ct = default);

    Task<Option<IReadOnlyCollection<StoryElementLayerDto>, Error>> GetStoryElementLayersAsync(Guid elementId, CancellationToken ct = default);
    Task<Option<StoryElementLayerDto, Error>> CreateStoryElementLayerAsync(CreateStoryElementLayerRequest request, CancellationToken ct = default);
    Task<Option<StoryElementLayerDto, Error>> UpdateStoryElementLayerAsync(Guid storyElementLayerId, UpdateStoryElementLayerRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteStoryElementLayerAsync(Guid storyElementLayerId, CancellationToken ct = default);

    Task<Option<ExportedStoryDto, Error>> ExportAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<bool, Error>> ImportAsync(ImportStoryRequest request, CancellationToken ct = default);
}
