using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Locations.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.POIs;

public record PoiDto(
    Guid PoiId,
    Guid MapId,
    Guid? SegmentId,
    Guid? ZoneId,
    string Title,
    string? Subtitle,
    LocationType LocationType,
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
