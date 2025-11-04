using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Domain.Entities.Locations;

public class Location
{
    public Guid LocationId { get; set; }
    public Guid SegmentId { get; set; }
    public Guid CreatedBy { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public LocationType LocationType { get; set; } = LocationType.PointOfInterest;
    public int DisplayOrder { get; set; } = 0;
    
    public string MarkerGeometry { get; set; } = string.Empty;
    
    public string? IconType { get; set; }
    public string? IconUrl { get; set; }
    public string? IconColor { get; set; }
    public int IconSize { get; set; } = 32;
    public int ZIndex { get; set; } = 100;
    
    public bool ShowTooltip { get; set; } = true;
    public string? TooltipContent { get; set; }
    public bool OpenPopupOnClick { get; set; } = false;
    public string? PopupContent { get; set; }
    
    public string? MediaUrls { get; set; }
    public bool PlayAudioOnClick { get; set; } = false;
    public string? AudioUrl { get; set; }
    
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    public string? ExitEffect { get; set; } = "fade";
    
    public Guid? LinkedSegmentId { get; set; }
    public Guid? LinkedLocationId { get; set; }
    public string? ExternalUrl { get; set; }
    
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Segment? Segment { get; set; }
    public User? Creator { get; set; }
    public Segment? LinkedSegment { get; set; }
    public Location? LinkedLocation { get; set; }
}