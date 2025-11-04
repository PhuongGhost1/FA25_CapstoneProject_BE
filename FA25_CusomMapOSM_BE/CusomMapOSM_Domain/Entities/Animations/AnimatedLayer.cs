using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Animations;

public class AnimatedLayer
{
    public Guid AnimatedLayerId { get; set; }
    public Guid CreatedBy { get; set; }
    
    public Guid? LayerId { get; set; }        // Nullable - nếu thuộc layer
    public Guid? SegmentId { get; set; }      // Nullable - nếu thuộc segment
    
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
    public string? CssFilter { get; set; }
    
    // Playback
    public bool AutoPlay { get; set; } = true;
    public bool Loop { get; set; } = true;
    public int PlaybackSpeed { get; set; } = 100;
    public int StartTimeMs { get; set; } = 0;
    public int? EndTimeMs { get; set; }
    
    // Entry/Exit animation
    public int EntryDelayMs { get; set; } = 0;
    public int EntryDurationMs { get; set; } = 400;
    public string? EntryEffect { get; set; } = "fade";
    
    public int ExitDelayMs { get; set; } = 0;
    public int ExitDurationMs { get; set; } = 400;
    public string? ExitEffect { get; set; } = "fade";
    
    // Interaction
    public bool EnableClick { get; set; } = false;
    public string? OnClickAction { get; set; }
    
    // Metadata
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Layer? Layer { get; set; }
    public Segment? Segment { get; set; }
    public User? Creator { get; set; }
}