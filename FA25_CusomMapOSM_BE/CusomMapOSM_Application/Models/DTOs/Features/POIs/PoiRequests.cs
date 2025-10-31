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
    string? AnimationOverrides = null,
    bool IsVisible = true,
    int ZIndex = 0);

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
    string? AnimationOverrides = null,
    bool? IsVisible = null,
    int? ZIndex = null);

public record UpdatePoiDisplayConfigRequest(
    bool? IsVisible,
    int? ZIndex,
    bool? ShowTooltip,
    string? TooltipContent);

public record UpdatePoiInteractionConfigRequest(
    bool? OpenSlideOnClick,
    bool? PlayAudioOnClick,
    string? AudioUrl,
    string? ExternalUrl);
