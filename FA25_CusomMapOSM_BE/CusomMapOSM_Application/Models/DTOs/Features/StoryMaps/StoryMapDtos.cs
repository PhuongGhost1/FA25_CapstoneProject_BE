using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Timeline.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

public record SegmentDto(
    Guid SegmentId,
    Guid MapId,
    string Name,
    string? Summary,
    string? StoryContent,
    int DisplayOrder,
    bool AutoFitBounds,
    Guid? EntryAnimationPresetId,
    Guid? ExitAnimationPresetId,
    Guid? DefaultLayerAnimationPresetId,
    SegmentPlaybackMode PlaybackMode,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<SegmentZoneDto> Zones,
    IReadOnlyCollection<SegmentLayerDto> Layers,
    IReadOnlyCollection<PoiDto> Locations);

public record SegmentZoneDto(
    Guid SegmentZoneId,
    Guid SegmentId,
    string Name,
    string? Description,
    SegmentZoneType ZoneType,
    string ZoneGeometry,
    string? FocusCameraState,
    int DisplayOrder,
    bool IsPrimary,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SegmentLayerDto(
    Guid SegmentLayerId,
    Guid SegmentId,
    Guid LayerId,
    Guid? SegmentZoneId,
    bool ExpandToZone,
    bool HighlightZoneBoundary,
    int DisplayOrder,
    int DelayMs,
    int FadeInMs,
    int FadeOutMs,
    double StartOpacity,
    double EndOpacity,
    AnimationEasingType Easing,
    Guid? AnimationPresetId,
    bool AutoPlayAnimation,
    int RepeatCount,
    string? AnimationOverrides,
    string? OverrideStyle,
    string? Metadata);

public record TimelineStepDto(
    Guid TimelineStepId,
    Guid MapId,
    Guid? SegmentId,
    string Title,
    string? Subtitle,
    string? Description,
    int DisplayOrder,
    bool AutoAdvance,
    int DurationMs,
    TimelineTriggerType TriggerType,
    string? CameraState,
    string? OverlayContent,
    DateTime CreatedAt,
    IReadOnlyCollection<TimelineStepLayerDto> Layers);

public record TimelineStepLayerDto(
    Guid TimelineStepLayerId,
    Guid TimelineStepId,
    Guid LayerId,
    bool IsVisible,
    double Opacity,
    int FadeInMs,
    int FadeOutMs,
    int DelayMs,
    TimelineLayerDisplayMode DisplayMode,
    string? StyleOverride,
    string? Metadata);

public record ZoneAnalyticsResponse(
    IReadOnlyCollection<ZoneAnalyticsItem> Zones);

public record ZoneAnalyticsItem(
    Guid ZoneId,
    string Name,
    ZoneAdminLevel AdminLevel,
    string? ZoneCode,
    string? Geometry,
    string? SimplifiedGeometry,
    string? Centroid,
    string? BoundingBox,
    IReadOnlyCollection<ZoneStatisticItem> Statistics,
    IReadOnlyCollection<ZoneInsightItem> Insights);

public record ZoneStatisticItem(
    Guid ZoneStatisticId,
    ZoneMetricType MetricType,
    double? NumericValue,
    string? TextValue,
    string? Unit,
    int? Year,
    int? Quarter,
    string? Source,
    string? Metadata,
    DateTime CollectedAt);

public record ZoneInsightItem(
    Guid ZoneInsightId,
    ZoneInsightType InsightType,
    string Title,
    string? Summary,
    string? Description,
    string? ImageUrl,
    string? ExternalUrl,
    string? Location,
    string? Metadata,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
