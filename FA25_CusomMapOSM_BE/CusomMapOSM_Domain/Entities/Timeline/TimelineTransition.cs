using System;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline.Enums;

namespace CusomMapOSM_Domain.Entities.Timeline;

public class TimelineTransition
{
    public Guid TimelineTransitionId { get; set; }
    public Guid MapId { get; set; }
    public Guid FromSegmentId { get; set; }
    public Guid ToSegmentId { get; set; }
    
    public string? TransitionName { get; set; }
    public int DurationMs { get; set; } = 1000;
    public TransitionType TransitionType { get; set; } = TransitionType.Ease;
    
    public bool AnimateCamera { get; set; } = true;
    public CameraAnimationType CameraAnimationType { get; set; } = CameraAnimationType.Fly;
    public int CameraAnimationDurationMs { get; set; } = 1000;
    
    public bool ShowOverlay { get; set; } = false;
    public string? OverlayContent { get; set; }
    
    public bool AutoTrigger { get; set; } = true;
    public bool RequireUserAction { get; set; } = false;
    public string? TriggerButtonText { get; set; } = "Next";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Map? Map { get; set; }
    public Segment? FromSegment { get; set; }
    public Segment? ToSegment { get; set; }
}