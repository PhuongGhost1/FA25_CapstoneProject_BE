using System;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Domain.Entities.Timeline;

public class TimelineStep
{
    public Guid TimelineStepId { get; set; }
    public Guid MapId { get; set; }
    public Guid? SegmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool AutoAdvance { get; set; } = true;
    public int DurationMs { get; set; } = 6000;
    public TimelineTriggerType TriggerType { get; set; } = TimelineTriggerType.Auto;
    public string? CameraState { get; set; }                   // JSON: zoom, center, bearing
    public string? OverlayContent { get; set; }                // HTML/Markdown shown with step
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Map? Map { get; set; }
    public MapSegment? Segment { get; set; }
}
