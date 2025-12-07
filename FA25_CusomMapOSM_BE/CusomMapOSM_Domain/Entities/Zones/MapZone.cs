using System;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Domain.Entities.Zones;


public class MapZone
{
    public Guid MapZoneId { get; set; }
    public Guid MapId { get; set; }
    public Guid ZoneId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    public bool HighlightBoundary { get; set; } = true;
    public string? BoundaryColor { get; set; }
    public int BoundaryWidth { get; set; } = 2;
    public bool FillZone { get; set; } = false;
    public string? FillColor { get; set; }
    public decimal FillOpacity { get; set; } = 0.3m;
    public bool ShowLabel { get; set; } = true;
    public string? LabelOverride { get; set; }
    public string? LabelStyle { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Map? Map { get; set; }
    public Zone? Zone { get; set; }
}