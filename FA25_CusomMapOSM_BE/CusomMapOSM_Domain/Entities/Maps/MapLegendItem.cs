using System;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

/// <summary>
/// Custom legend item for map - allows users to define their own legend entries
/// </summary>
public class MapLegendItem
{
    public Guid LegendItemId { get; set; }
    public Guid MapId { get; set; }
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Display label for the legend item
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description shown on hover/tooltip
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Emoji character for the icon (e.g., "📍", "🏛️")
    /// </summary>
    public string Emoji { get; set; } = "📍";
    
    /// <summary>
    /// Custom icon URL (if using custom image instead of emoji)
    /// </summary>
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// Optional color for the legend item (hex color code)
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Display order in the legend panel
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether the legend item is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Map? Map { get; set; }
    public User? Creator { get; set; }
}
