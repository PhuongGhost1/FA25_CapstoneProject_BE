using System;
using CusomMapOSM_Domain.Entities.Layers.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Layers;

public class LayerSummaryDto
{
    public Guid LayerId { get; set; }
    public Guid MapId { get; set; }
    public string LayerName { get; set; } = string.Empty;
    public LayerType LayerType { get; set; }
    public LayerSource SourceType { get; set; }
    public bool IsPublic { get; set; }
    public int? FeatureCount { get; set; }
    public double? DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

