using System;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Segments.Enums;

namespace CusomMapOSM_Domain.Entities.Segments;

public class SegmentLayer
{
    public Guid SegmentLayerId { get; set; }
    public Guid SegmentId { get; set; }
    public Guid LayerId { get; set; }
    public Guid? SegmentZoneId { get; set; }
    public bool ExpandToZone { get; set; } = true;
    public bool HighlightZoneBoundary { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int DelayMs { get; set; }
    public int FadeInMs { get; set; } = 400;
    public int FadeOutMs { get; set; } = 400;
    public double StartOpacity { get; set; } = 0.0;
    public double EndOpacity { get; set; } = 1.0;
    public AnimationEasingType Easing { get; set; } = AnimationEasingType.EaseOut;
    public Guid? AnimationPresetId { get; set; }
    public bool AutoPlayAnimation { get; set; } = true;
    public int RepeatCount { get; set; } = 1;                  // 0 = infinite
    public string? AnimationOverrides { get; set; }            // JSON overrides for preset
    public string? OverrideStyle { get; set; }                 // JSON style override
    public string? Metadata { get; set; }                      // optional: extra info

    public Segment? Segment { get; set; }
    public SegmentZone? SegmentZone { get; set; }
    public LayerAnimationPreset? AnimationPreset { get; set; }
    public Layer? Layer { get; set; }
}
