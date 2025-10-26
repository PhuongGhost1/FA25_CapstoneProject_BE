using System;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Domain.Entities.Animations;

public class LayerAnimation
{
    public Guid LayerAnimationId { get; set; }
    public Guid LayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string? Coordinates { get; set; }
    public double RotationDeg { get; set; } = 0.0;
    public double Scale { get; set; } = 1.0;
    public int ZIndex { get; set; } = 1000;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Layer? Layer { get; set; }
}