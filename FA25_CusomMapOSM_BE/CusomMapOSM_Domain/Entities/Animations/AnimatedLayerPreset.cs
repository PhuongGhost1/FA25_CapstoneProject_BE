using CusomMapOSM_Domain.Entities.Animations.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Animations;

public class AnimatedLayerPreset
{
    public Guid AnimatedLayerPresetId { get; set; }
    public Guid? CreatedBy { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    
    public AnimatedLayerType MediaType { get; set; } = AnimatedLayerType.GIF;
    public string SourceUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    public string? DefaultCoordinates { get; set; }
    public bool DefaultIsScreenOverlay { get; set; } = false;
    public string? DefaultScreenPosition { get; set; }
    public double DefaultScale { get; set; } = 1.0;
    public decimal DefaultOpacity { get; set; } = 1.0m;
    public bool DefaultAutoPlay { get; set; } = true;
    public bool DefaultLoop { get; set; } = true;
    
    public bool IsSystemPreset { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public User? Creator { get; set; }
}