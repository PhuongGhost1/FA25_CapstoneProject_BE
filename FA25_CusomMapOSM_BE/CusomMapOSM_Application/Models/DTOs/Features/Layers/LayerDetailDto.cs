using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Layers;

public class LayerDetailDto : LayerSummaryDto
{
    public Guid UserId { get; set; }
    public string? FilePath { get; set; }
    public string? DataStoreKey { get; set; }
    public string? LayerData { get; set; }
    public string? LayerStyle { get; set; }
    public bool IsVisible { get; set; }
}

