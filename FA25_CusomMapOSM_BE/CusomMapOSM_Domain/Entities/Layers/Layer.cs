using System;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Layers;

public class Layer
{
    public Guid LayerId { get; set; }
    public Guid UserId { get; set; }
    public string? LayerName { get; set; }
    public LayerTypeEnum LayerType { get; set; }
    public LayerSourceEnum SourceType { get; set; }
    public string? FilePath { get; set; }
    public string? LayerData { get; set; }
    public string? LayerStyle { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public User? User { get; set; }

}
