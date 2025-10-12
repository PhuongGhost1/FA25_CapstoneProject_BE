using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;


public class UpdateLayerDataRequest
{

    [Required(ErrorMessage = "Layer data is required")]
    public string LayerData { get; set; } = string.Empty;
}

