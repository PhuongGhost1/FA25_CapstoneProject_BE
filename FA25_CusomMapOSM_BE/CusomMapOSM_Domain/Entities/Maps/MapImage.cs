using System;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapImage
{
    public int MapImageId { get; set; }
    public Guid MapId { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageData { get; set; }                       
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Rotation { get; set; }
    public int ZIndex { get; set; } = 500;
    public bool IsVisible { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Map? Map { get; set; }
}
