using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Models.DTOs.Features.Locations;

public class CreateLocationRequest
{
    public Guid MapId { get; set; }
    public string? SegmentId { get; set; }
    public string? ZoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public string MarkerGeometry { get; set; } = string.Empty;
    public string? MediaResources { get; set; }
    public int DisplayOrder { get; set; }
    public string? Rotation  { get; set; }
    public string? IconType { get; set; }
    public string? IconColor { get; set; }
    public int IconSize { get; set; } = 32;
    public int ZIndex { get; set; } = 100;
    public bool ShowTooltip { get; set; } = true;
    public string? TooltipContent { get; set; }
    public string? IconUrl { get; set; }
    public string? AudioUrl { get; set; }
    public bool OpenPopupOnClick { get; set; } = false;
    public string? PopupContent { get; set; }
    public bool PlayAudioOnClick { get; set; } = false;
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    public string? ExitEffect { get; set; } = "fade";
    public string? LinkedLocationId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public IFormFile? AudioFile { get; set; }
    public IFormFile? IconFile { get; set; }
}

public class UpdateLocationRequest
{
    public string? SegmentId { get; set; }
    public string? ZoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public string MarkerGeometry { get; set; } = string.Empty;
    public string? MediaResources { get; set; }
    public int DisplayOrder { get; set; }
    public string? Rotation { get; set; }
    public string? IconUrl { get; set; }
    public string? IconType { get; set; }
    public string? IconColor { get; set; }
    public int IconSize { get; set; } = 32;
    public int ZIndex { get; set; } = 100;
    public bool ShowTooltip { get; set; } = true;
    public string? TooltipContent { get; set; }
    public bool OpenPopupOnClick { get; set; } = false;
    public string? PopupContent { get; set; }
    public bool PlayAudioOnClick { get; set; } = false;
    public string? AudioUrl { get; set; }
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    public string? ExitEffect { get; set; } = "fade";
    public string? LinkedLocationId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public IFormFile? AudioFile { get; set; }
    public IFormFile? IconFile { get; set; }
}

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
