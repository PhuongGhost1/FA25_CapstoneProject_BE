using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;


public class CreateLayerRequest
{

    [Required(ErrorMessage = "Layer name is required")]
    [StringLength(100, ErrorMessage = "Layer name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;


    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }


    [Required(ErrorMessage = "Layer type is required")]
    public string LayerType { get; set; } = string.Empty;


    public string? LayerData { get; set; }


    public string? LayerStyle { get; set; }


    public bool IsVisible { get; set; } = true;
}
