using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Models.DTOs.Features.Locations;

public record CreateLocationRequest(
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
    Guid? LinkedLocationId = null,
    bool PlayAudioOnClick = false,
    string? AudioUrl = null,
    string? ExternalUrl = null,
    Guid? AssociatedLayerId = null,
    Guid? AnimationPresetId = null,
    string? AnimationOverrides = null,
    bool IsVisible = true,
    int ZIndex = 0,
    IFormFile? IconFile = null);

public record UpdateLocationRequest(
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
    Guid? LinkedLocationId = null,
    bool PlayAudioOnClick = false,
    string? AudioUrl = null,
    string? ExternalUrl = null,
    Guid? AssociatedLayerId = null,
    Guid? AnimationPresetId = null,
    string? AnimationOverrides = null,
    bool? IsVisible = null,
    int? ZIndex = null);

public record UpdateLocationDisplayConfigRequest(
    bool? IsVisible,
    int? ZIndex,
    bool? ShowTooltip,
    string? TooltipContent);

public record UpdateLocationInteractionConfigRequest(
    bool? OpenSlideOnClick,
    bool? PlayAudioOnClick,
    string? AudioUrl,
    string? ExternalUrl);
