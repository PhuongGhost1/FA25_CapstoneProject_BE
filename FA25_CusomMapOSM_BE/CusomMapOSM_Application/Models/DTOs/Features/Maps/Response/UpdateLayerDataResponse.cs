namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;


public class UpdateLayerDataResponse
{

    public bool Success { get; set; }


    public string Message { get; set; } = string.Empty;


    public int FeatureCount { get; set; }
}

