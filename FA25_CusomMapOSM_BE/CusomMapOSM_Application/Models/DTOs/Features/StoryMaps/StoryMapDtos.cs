using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.StoryElement.Enums;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Domain.Entities.Animations.Enums;

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
    Guid ZoneId,
    Guid? SegmentId,
    string Name,
    string? Description,
    ZoneType ZoneType,
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
    Guid? ZoneId,
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

public record SegmentTransitionDto(
    Guid SegmentTransitionId,
    Guid FromSegmentId,
    Guid ToSegmentId,
    TransitionEffectType EffectType,
    Guid? AnimationPresetId,
    int DurationMs,
    int DelayMs,
    bool AutoPlay,
    bool IsSkippable,
    string? TransitionConfig,
    string? Metadata);

public record TransitionPreviewDto(
    Guid FromSegmentId,
    Guid ToSegmentId,
    string? FromCameraState,
    string? ToCameraState,
    int SuggestedDurationMs,
    string Easing);

public record StoryElementLayerDto(
    Guid StoryElementLayerId,
    Guid ElementId,
    StoryElementType ElementType,
    Guid LayerId,
    Guid? ZoneId,
    bool ExpandToZone,
    bool HighlightZoneBoundary,
    int DisplayOrder,
    int DelayMs,
    int FadeInMs,
    int FadeOutMs,
    decimal StartOpacity,
    decimal EndOpacity,
    AnimationEasingType Easing,
    Guid? AnimationPresetId,
    bool AutoPlayAnimation,
    int RepeatCount,
    string? AnimationOverrides,
    string? OverrideStyle,
    string? Metadata,
    bool IsVisible,
    decimal Opacity,
    StoryElementDisplayMode DisplayMode,
    string? StyleOverride,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ExportedStoryDto(
    Guid MapId,
    IReadOnlyCollection<SegmentDto> Segments,
    IReadOnlyCollection<TimelineStepDto> Timeline);
