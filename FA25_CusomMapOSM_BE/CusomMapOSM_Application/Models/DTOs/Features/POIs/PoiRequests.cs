using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Locations.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.POIs;

public record CreatePoiRequest(
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
    bool ShowTooltip = true,
    string? TooltipContent = null,
    string? EffectType = null,
    bool OpenSlideOnClick = false,
    string? SlideContent = null,
    Guid? LinkedPoiId = null,
    bool PlayAudioOnClick = false,
    string? AudioUrl = null,
    string? ExternalUrl = null,
    Guid? AssociatedLayerId = null,
    Guid? AnimationPresetId = null,
    string? AnimationOverrides = null);

public record UpdatePoiRequest(
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
    bool ShowTooltip = true,
    string? TooltipContent = null,
    string? EffectType = null,
    bool OpenSlideOnClick = false,
    string? SlideContent = null,
    Guid? LinkedPoiId = null,
    bool PlayAudioOnClick = false,
    string? AudioUrl = null,
    string? ExternalUrl = null,
    Guid? AssociatedLayerId = null,
    Guid? AnimationPresetId = null,
    string? AnimationOverrides = null);
