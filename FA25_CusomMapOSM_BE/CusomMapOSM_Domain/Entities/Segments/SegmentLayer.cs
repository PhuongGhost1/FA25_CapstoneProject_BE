using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Domain.Entities.Segments;

public class SegmentLayer
{
    public Guid SegmentLayerId { get; set; }
    public Guid SegmentId { get; set; }
    public Guid LayerId { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public decimal Opacity { get; set; } = 1.0m;
    public int ZIndex { get; set; } = 0;
    
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    public string? ExitEffect { get; set; } = "fade";
    
    public string? StyleOverride { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Segment? Segment { get; set; }
    public Layer? Layer { get; set; }
}
