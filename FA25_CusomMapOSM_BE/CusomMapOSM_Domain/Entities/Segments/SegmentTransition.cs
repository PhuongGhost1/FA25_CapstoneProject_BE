using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Segments.Enums;

namespace CusomMapOSM_Domain.Entities.Segments;

public class SegmentTransition
{
    public Guid SegmentTransitionId { get; set; }
    public Guid FromSegmentId { get; set; }
    public Guid ToSegmentId { get; set; }
    public TransitionEffectType EffectType { get; set; } = TransitionEffectType.Fade;
    public Guid? AnimationPresetId { get; set; }
    public int DurationMs { get; set; } = 600;
    public int DelayMs { get; set; }
    public bool AutoPlay { get; set; } = true;
    public bool IsSkippable { get; set; } = true;
    public string? TransitionConfig { get; set; }              // JSON override values
    public string? Metadata { get; set; }

    public Segment? FromSegment { get; set; }
    public Segment? ToSegment { get; set; }
    public LayerAnimationPreset? AnimationPreset { get; set; }
}
