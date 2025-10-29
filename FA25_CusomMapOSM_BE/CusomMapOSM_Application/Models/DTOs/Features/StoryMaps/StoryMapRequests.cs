using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.StoryElement.Enums;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
using CusomMapOSM_Domain.Entities.Zones.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

public record CreateSegmentRequest(
    Guid MapId,
    string Name,
    string? Summary,
    string? StoryContent,
    int DisplayOrder,
    bool AutoFitBounds,
    Guid? EntryAnimationPresetId,
    Guid? ExitAnimationPresetId,
    Guid? DefaultLayerAnimationPresetId,
    SegmentPlaybackMode PlaybackMode);

public record UpdateSegmentRequest(
    string Name,
    string? Summary,
    string? StoryContent,
    int DisplayOrder,
    bool AutoFitBounds,
    Guid? EntryAnimationPresetId,
    Guid? ExitAnimationPresetId,
    Guid? DefaultLayerAnimationPresetId,
    SegmentPlaybackMode PlaybackMode);

public record CreateSegmentZoneRequest(
    Guid SegmentId,
    string Name,
    string? Description,
    ZoneType ZoneType,
    string ZoneGeometry,
    string? FocusCameraState,
    int DisplayOrder,
    bool IsPrimary);

public record UpdateSegmentZoneRequest(
    string Name,
    string? Description,
    ZoneType ZoneType,
    string ZoneGeometry,
    string? FocusCameraState,
    int DisplayOrder,
    bool IsPrimary);

public record UpsertSegmentLayerRequest(
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
    string? StyleOverride,
    string? Metadata);

public record CreateTimelineStepRequest(
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
    IReadOnlyCollection<CreateTimelineStepLayerRequest> Layers);

public record UpdateTimelineStepRequest(
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
    IReadOnlyCollection<CreateTimelineStepLayerRequest> Layers);

public record CreateTimelineStepLayerRequest(
    Guid LayerId,
    bool IsVisible,
    decimal Opacity,
    int FadeInMs,
    int FadeOutMs,
    int DelayMs,
    TimelineLayerDisplayMode DisplayMode,
    string? StyleOverride,
    string? Metadata);

public record CreateSegmentTransitionRequest(
    Guid MapId,
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

public record UpdateSegmentTransitionRequest(
    TransitionEffectType EffectType,
    Guid? AnimationPresetId,
    int DurationMs,
    int DelayMs,
    bool AutoPlay,
    bool IsSkippable,
    string? TransitionConfig,
    string? Metadata);

public record PreviewTransitionRequest(
    Guid FromSegmentId,
    Guid ToSegmentId);

public record CreateStoryElementLayerRequest(
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
    string? Metadata,
    bool IsVisible,
    decimal Opacity,
    StoryElementDisplayMode DisplayMode,
    string? StyleOverride);

public record UpdateStoryElementLayerRequest(
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
    string? Metadata,
    bool IsVisible,
    decimal Opacity,
    StoryElementDisplayMode DisplayMode,
    string? StyleOverride);

public record ImportStoryRequest(
    Guid MapId,
    IReadOnlyCollection<SegmentDto> Segments,
    IReadOnlyCollection<TimelineStepDto> Timeline);
