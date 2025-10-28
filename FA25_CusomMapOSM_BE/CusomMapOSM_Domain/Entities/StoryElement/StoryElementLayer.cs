using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.StoryElement.Enums;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Domain.Entities.StoryElement;

public class StoryElementLayer
{
    public Guid StoryElementLayerId { get; set; }
    public Guid ElementId { get; set; }
    public StoryElementType ElementType { get; set; }
    public Guid LayerId { get; set; }
    public Guid? ZoneId { get; set; }
    public bool ExpandToZone { get; set; } = false;
    public bool HighlightZoneBoundary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public int DelayMs { get; set; } = 0;
    public int FadeInMs { get; set; } = 400;
    public int FadeOutMs { get; set; } = 400;
    public decimal StartOpacity { get; set; } = 0.0m;
    public decimal EndOpacity { get; set; } = 1.0m;
    public AnimationEasingType Easing { get; set; } = AnimationEasingType.EaseOut;
    public Guid? AnimationPresetId { get; set; }
    public bool AutoPlayAnimation { get; set; } = true;
    public int RepeatCount { get; set; } = 1; // 0 = infinite
    public string? AnimationOverrides { get; set; } // JSON overrides for preset
    public string? OverrideStyle { get; set; } // JSON style override
    public string? Metadata { get; set; } // Optional extra info
    public bool IsVisible { get; set; } = true;
    public decimal Opacity { get; set; } = 1.0m;
    public StoryElementDisplayMode DisplayMode { get; set; } = StoryElementDisplayMode.Normal;
    public string? StyleOverride { get; set; } // JSON style override (duplicate field from schema)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Layer? Layer { get; set; }
    public Zone? Zone { get; set; }
    public LayerAnimationPreset? AnimationPreset { get; set; }
}
