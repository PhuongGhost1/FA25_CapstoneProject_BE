using System.Text.Json.Serialization;
using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
using CusomMapOSM_Domain.Entities.Zones.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

public record CreateSegmentRequest(
    Guid MapId,
    string Name,
    string? Description,
    string? StoryContent,
    int DisplayOrder,
    string? CameraState,
    bool AutoAdvance,
    int DurationMs,
    bool RequireUserAction,
    SegmentPlaybackMode PlaybackMode
);

public record UpdateSegmentRequest(
    string Name,
    string? Description,
    string? StoryContent,
    int? DisplayOrder,
    string? CameraState,
    bool? AutoAdvance,
    int? DurationMs,
    bool? RequireUserAction,
    SegmentPlaybackMode? PlaybackMode);

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
    Guid? AnimationPresetId,
    bool AutoPlayAnimation,
    int RepeatCount,
    string? AnimationOverrides,
    string? Metadata,
    bool IsVisible,
    decimal Opacity,
    string? StyleOverride);

public record UpdateStoryElementLayerRequest(
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
    Guid? AnimationPresetId,
    bool AutoPlayAnimation,
    int RepeatCount,
    string? AnimationOverrides,
    string? Metadata,
    bool IsVisible,
    decimal Opacity,
    string? StyleOverride);

public record ImportStoryRequest(
    Guid MapId,
    IReadOnlyCollection<SegmentDto> Segments,
    IReadOnlyCollection<TimelineStepDto> Timeline);

// ================== TIMELINE TRANSITION ==================
public record CreateTimelineTransitionRequest(
    Guid MapId,
    Guid FromSegmentId,
    Guid ToSegmentId,
    string? TransitionName,
    int DurationMs,
    TransitionType TransitionType,
    bool AnimateCamera,
    CameraAnimationType CameraAnimationType,
    int CameraAnimationDurationMs,
    bool ShowOverlay,
    string? OverlayContent,
    bool AutoTrigger,
    bool RequireUserAction,
    string? TriggerButtonText);

public record UpdateTimelineTransitionRequest(
    string? TransitionName,
    int DurationMs,
    TransitionType TransitionType,
    bool AnimateCamera,
    CameraAnimationType CameraAnimationType,
    int CameraAnimationDurationMs,
    bool ShowOverlay,
    string? OverlayContent,
    bool AutoTrigger,
    bool RequireUserAction,
    string? TriggerButtonText);

// ================== ANIMATED LAYER ==================
public record CreateAnimatedLayerRequest(
    Guid? LayerId,
    Guid? SegmentId,
    string Name,
    string? Description,
    int DisplayOrder,
    AnimatedLayerType MediaType,
    string SourceUrl,
    string? ThumbnailUrl,
    string? Coordinates,
    bool IsScreenOverlay,
    string? ScreenPosition,
    double RotationDeg,
    double Scale,
    decimal Opacity,
    int ZIndex,
    string? CssFilter,
    bool AutoPlay,
    bool Loop,
    int PlaybackSpeed,
    int StartTimeMs,
    int? EndTimeMs,
    int EntryDelayMs,
    int EntryDurationMs,
    string? EntryEffect,
    int ExitDelayMs,
    int ExitDurationMs,
    string? ExitEffect,
    bool EnableClick,
    string? OnClickAction);

public record UpdateAnimatedLayerRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    string? Coordinates,
    bool IsScreenOverlay,
    string? ScreenPosition,
    double RotationDeg,
    double Scale,
    decimal Opacity,
    int ZIndex,
    string? CssFilter,
    bool AutoPlay,
    bool Loop,
    int PlaybackSpeed,
    int StartTimeMs,
    int? EndTimeMs,
    int EntryDelayMs,
    int EntryDurationMs,
    string? EntryEffect,
    int ExitDelayMs,
    int ExitDurationMs,
    string? ExitEffect,
    bool EnableClick,
    string? OnClickAction,
    bool IsVisible);

// ================== ANIMATED LAYER PRESET ==================
public record CreateAnimatedLayerPresetRequest(
    string Name,
    string? Description,
    string? Category,
    string? Tags,
    AnimatedLayerType MediaType,
    string SourceUrl,
    string ThumbnailUrl,
    string? DefaultCoordinates,
    bool DefaultIsScreenOverlay,
    string? DefaultScreenPosition,
    double DefaultScale,
    decimal DefaultOpacity,
    bool DefaultAutoPlay,
    bool DefaultLoop,
    bool IsSystemPreset,
    bool IsPublic);

public record UpdateAnimatedLayerPresetRequest(
    string Name,
    string? Description,
    string? Category,
    string? Tags,
    string? DefaultCoordinates,
    bool DefaultIsScreenOverlay,
    string? DefaultScreenPosition,
    double DefaultScale,
    decimal DefaultOpacity,
    bool DefaultAutoPlay,
    bool DefaultLoop,
    bool IsPublic,
    bool IsActive);

// ================== ZONE (Master data) ==================
public record CreateZoneRequest(
    string ExternalId,
    string ZoneCode,
    string Name,
    ZoneType ZoneType,
    ZoneAdminLevel AdminLevel,
    Guid? ParentZoneId,
    string Geometry,
    string? SimplifiedGeometry,
    string? Centroid,
    string? BoundingBox,
    string? Description);

public record UpdateZoneRequest(
    string Name,
    string? Description,
    string? SimplifiedGeometry,
    string? Centroid,
    string? BoundingBox,
    bool IsActive);

public record SyncZonesFromOSMRequest(
    string AdminLevel,
    string? CountryCode,
    bool UpdateExisting);

// ================== SEGMENT ZONE ==================
public record CreateSegmentZoneV2Request(
    Guid SegmentId,
    Guid ZoneId,
    int DisplayOrder,
    bool IsVisible,
    int ZIndex,
    bool HighlightBoundary,
    string? BoundaryColor,
    int BoundaryWidth,
    bool FillZone,
    string? FillColor,
    decimal FillOpacity,
    bool ShowLabel,
    string? LabelOverride,
    string? LabelStyle,
    int EntryDelayMs,
    int EntryDurationMs,
    int ExitDelayMs,
    int ExitDurationMs,
    string? EntryEffect,
    string? ExitEffect,
    bool FitBoundsOnEntry,
    string? CameraOverride);

public record UpdateSegmentZoneV2Request(
    int DisplayOrder,
    bool IsVisible,
    int ZIndex,
    bool HighlightBoundary,
    string? BoundaryColor,
    int BoundaryWidth,
    bool FillZone,
    string? FillColor,
    decimal FillOpacity,
    bool ShowLabel,
    string? LabelOverride,
    string? LabelStyle,
    int EntryDelayMs,
    int EntryDurationMs,
    int ExitDelayMs,
    int ExitDurationMs,
    string? EntryEffect,
    string? ExitEffect,
    bool FitBoundsOnEntry,
    string? CameraOverride);

// ================== SEGMENT LAYER ==================
public record CreateSegmentLayerRequest(
    Guid SegmentId,
    Guid LayerId,
    int DisplayOrder,
    bool IsVisible,
    decimal Opacity,
    int ZIndex,
    int EntryDelayMs,
    int EntryDurationMs,
    int ExitDelayMs,
    int ExitDurationMs,
    string? EntryEffect,
    string? ExitEffect,
    string? StyleOverride);

public record UpdateSegmentLayerRequest(
    int DisplayOrder,
    bool IsVisible,
    decimal Opacity,
    int ZIndex,
    int EntryDelayMs,
    int EntryDurationMs,
    int ExitDelayMs,
    int ExitDurationMs,
    string? EntryEffect,
    string? ExitEffect,
    string? StyleOverride);
