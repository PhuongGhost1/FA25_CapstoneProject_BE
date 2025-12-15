using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class CopyFeatureToLayerRequest
{

    public string? TargetLayerId { get; set; }

    public string? NewLayerName { get; set; }

    [Required(ErrorMessage = "Feature index is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Feature index must be non-negative")]
    public int FeatureIndex { get; set; }
}

