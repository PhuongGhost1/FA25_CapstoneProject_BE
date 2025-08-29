using System;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapAnnotation
{
    public int MapAnnotationId { get; set; }
    public Guid MapId { get; set; }
    public string AnnotationName { get; set; } = string.Empty;
    public int AnnotationTypeId { get; set; }
    public string? GeometryData { get; set; }  
    public string? Style { get; set; }
    public string? Content { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 1000;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Map? Map { get; set; }
}
