using System;
using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Services.GeoJson;

public class GeoJsonLayerData
{
    public string LayerData { get; set; } = string.Empty;
    public int FeatureCount { get; set; }
    public double DataSizeKB { get; set; }
    public string DataBounds { get; set; } = string.Empty;
    public string LayerStyle { get; set; } = string.Empty;
    public string GeometryType { get; set; } = string.Empty;
    public List<string> PropertyNames { get; set; } = new();
    public bool IsValid { get; set; }
    public string? ValidationErrors { get; set; }
}
