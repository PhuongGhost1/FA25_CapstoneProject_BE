using System;
using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.Documents;

public class MapFeatureDocument
{
    public string Id { get; set; } = string.Empty;
    public Guid MapId { get; set; }
    public Guid? LayerId { get; set; }
    public string? Name { get; set; }
    public string FeatureCategory { get; set; } = string.Empty;
    public string? AnnotationType { get; set; }
    public string GeometryType { get; set; } = string.Empty;
    public object? Geometry { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public Dictionary<string, object>? Style { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
}

