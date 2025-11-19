using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Domain.Entities.Animations.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

public record SegmentDto(
    Guid SegmentId,
    Guid MapId,
    string Name,
    string? Description,
    string? StoryContent,
    int DisplayOrder,
    string CameraState,
    bool AutoAdvance,
    int DurationMs,
    bool RequireUserAction,
    IReadOnlyCollection<SegmentZoneDto> Zones,
    IReadOnlyCollection<SegmentLayerDto> Layers,
    IReadOnlyCollection<PoiDto> Locations);

public record SegmentZoneDto(
    Guid SegmentZoneId,
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
    string? CameraOverride,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    ZoneSummaryDto? Zone);

public record SegmentLayerDto(
    Guid SegmentLayerId,
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
    string? StyleOverride,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

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
    string? StyleOverride,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ExportedStoryDto(
    Guid MapId,
    IReadOnlyCollection<SegmentDto> Segments,
    IReadOnlyCollection<TimelineStepDto> Timeline);

// ================== TIMELINE TRANSITION (Chuyển cảnh) ==================
public record RouteAnimationDto(
    Guid RouteAnimationId,
    Guid SegmentId,
    Guid MapId,
    double FromLat,
    double FromLng,
    string? FromName,
    double ToLat,
    double ToLng,
    string? ToName,
    string RoutePath, // GeoJSON LineString
    string IconType,
    string? IconUrl,
    int IconWidth,
    int IconHeight,
    string RouteColor,
    string VisitedColor,
    int RouteWidth,
    int DurationMs,
    int? StartDelayMs,
    string Easing,
    bool AutoPlay,
    bool Loop,
    bool IsVisible,
    int ZIndex,
    int DisplayOrder,
    int? StartTimeMs,
    int? EndTimeMs,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record TimelineTransitionDto(
    Guid TimelineTransitionId,
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
    string? TriggerButtonText,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ================== ANIMATED LAYER (GIF/Video overlay) ==================
public record AnimatedLayerDto(
    Guid AnimatedLayerId,
    Guid CreatedBy,
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
    string? OnClickAction,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ================== ANIMATED LAYER PRESET (Thư viện GIF) ==================
public record AnimatedLayerPresetDto(
    Guid AnimatedLayerPresetId,
    Guid? CreatedBy,
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
    bool IsPublic,
    int UsageCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ================== ZONE (Master data) ==================
public record ZoneDto(
    Guid ZoneId,
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
    string? Description,
    bool IsActive,
    DateTime LastSyncedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ZoneSummaryDto(
    Guid ZoneId,
    string Name,
    string ZoneCode,
    ZoneType ZoneType,
    ZoneAdminLevel AdminLevel,
    string? Geometry,
    string? Centroid,
    string? BoundingBox);
