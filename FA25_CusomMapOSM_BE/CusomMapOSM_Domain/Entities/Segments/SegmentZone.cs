using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Domain.Entities.Segments;

public class SegmentZone
{
    public Guid SegmentZoneId { get; set; }
    public Guid SegmentId { get; set; }
    public Guid ZoneId { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    
    public bool HighlightBoundary { get; set; } = true;
    public string? BoundaryColor { get; set; }
    public int BoundaryWidth { get; set; } = 2;
    public bool FillZone { get; set; } = false;
    public string? FillColor { get; set; }
    public decimal FillOpacity { get; set; } = 0.3m;
    
    public bool ShowLabel { get; set; } = true;
    public string? LabelOverride { get; set; }
    public string? LabelStyle { get; set; }
    
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    public string? ExitEffect { get; set; } = "fade";
    
    public bool FitBoundsOnEntry { get; set; } = false;
    public string? CameraOverride { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Segment? Segment { get; set; }
    public Zone? Zone { get; set; }
}