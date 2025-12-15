namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;


public class LayerInfoResponse
{

    public string LayerId { get; set; } = string.Empty;


    public string LayerName { get; set; } = string.Empty;


    public string? Description { get; set; }


    public string LayerType { get; set; } = string.Empty;


    public int FeatureCount { get; set; }


    public bool IsVisible { get; set; }


    public int ZIndex { get; set; }
}
