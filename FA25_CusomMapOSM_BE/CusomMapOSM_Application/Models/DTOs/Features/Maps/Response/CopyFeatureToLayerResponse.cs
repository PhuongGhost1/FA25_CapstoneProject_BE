namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;


public class CopyFeatureToLayerResponse
{

    public bool Success { get; set; }


    public string Message { get; set; } = string.Empty;


    public int TargetLayerFeatureCount { get; set; }


    public string TargetLayerId { get; set; } = string.Empty;


    public string TargetLayerName { get; set; } = string.Empty;


    public bool NewLayerCreated { get; set; }
}

