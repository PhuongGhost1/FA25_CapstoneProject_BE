using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Segments.Enums;

namespace CusomMapOSM_Domain.Entities.Segments;

public class LayerAnimationPreset
{
    public Guid AnimationPresetId { get; set; }
    public string Key { get; set; } = string.Empty;            // Unique identifier, e.g. "fade-in"
    public string DisplayName { get; set; } = string.Empty;
    public LayerAnimationType AnimationType { get; set; } = LayerAnimationType.FadeIn;
    public AnimationEasingType DefaultEasing { get; set; } = AnimationEasingType.EaseOut;
    public int DefaultDurationMs { get; set; } = 600;
    public string? Description { get; set; }
    public string? ConfigSchema { get; set; }                  // JSON schema for overrides
    public bool IsSystemPreset { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
