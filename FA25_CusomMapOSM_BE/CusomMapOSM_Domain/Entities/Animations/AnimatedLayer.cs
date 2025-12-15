using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Animations;

public class AnimatedLayer
{
    public Guid AnimatedLayerId { get; set; }
    public Guid? LayerId { get; set; }        // Nullable - nếu thuộc layer
    public Guid? SegmentId { get; set; }      // Nullable - nếu thuộc segment
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    
    public AnimatedLayerType MediaType { get; set; } = AnimatedLayerType.GIF;
    public string SourceUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    

    public string? Coordinates { get; set; }
    
    public bool IsScreenOverlay { get; set; } = false;
    public string? ScreenPosition { get; set; }  // JSON
    
    // Transform
    public double RotationDeg { get; set; } = 0.0;
    public double Scale { get; set; } = 1.0;
    public decimal Opacity { get; set; } = 1.0m;
    public int ZIndex { get; set; } = 1000;
    
    // Metadata
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Layer? Layer { get; set; }
    public Segment? Segment { get; set; }
    public User? Creator { get; set; }
}