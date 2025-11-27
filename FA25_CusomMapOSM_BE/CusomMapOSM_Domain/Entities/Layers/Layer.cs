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
    
    public string LayerName { get; set; } = string.Empty;
    public LayerType LayerType { get; set; }
    public LayerSource SourceType { get; set; }
    
    public string? FilePath { get; set; }
    public string? DataStoreKey { get; set; }
    public string? LayerData { get; set; }
    public string? LayerStyle { get; set; }
    public bool IsVisible { get; set; } = true;
    public int? FeatureCount { get; set; }
    public double? DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Map? Map { get; set; }
    public User? User { get; set; }
}