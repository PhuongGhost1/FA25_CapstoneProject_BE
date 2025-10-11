using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.POIs;

public record PoiDto(
    Guid PoiId,
    Guid MapId,
    Guid? SegmentId,
    Guid? SegmentZoneId,
    string Title,
    string? Subtitle,
    SegmentLocationType LocationType,
    string? MarkerGeometry,
    string? StoryContent,
    string? MediaResources,
    int DisplayOrder,
    bool HighlightOnEnter,
    bool ShowTooltip,
    string? TooltipContent,
    string? EffectType,
    bool OpenSlideOnClick,
    string? SlideContent,
    Guid? LinkedPoiId,
    bool PlayAudioOnClick,
    string? AudioUrl,
    string? ExternalUrl,
    Guid? AssociatedLayerId,
    Guid? AnimationPresetId,
    string? AnimationOverrides,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
