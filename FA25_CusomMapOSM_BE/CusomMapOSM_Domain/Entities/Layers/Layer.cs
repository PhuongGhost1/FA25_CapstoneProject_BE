using System;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Layers;

public class Layer
{
    public Guid LayerId { get; set; }
    public Guid MapId { get; set; }
    public Guid UserId { get; set; }
    public string? LayerName { get; set; }
    public LayerTypeEnum LayerType { get; set; }
    public LayerSourceEnum SourceType { get; set; }
    public string? FilePath { get; set; }
    public string? LayerData { get; set; }
    public string? LayerStyle { get; set; }
    public bool IsPublic { get; set; }
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    public int LayerOrder { get; set; } = 0;
    public string? CustomStyle { get; set; }
    public string? FilterConfig { get; set; }
    public int? FeatureCount { get; set; }
    public double? DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Map? Map { get; set; }
    public User? User { get; set; }
}
