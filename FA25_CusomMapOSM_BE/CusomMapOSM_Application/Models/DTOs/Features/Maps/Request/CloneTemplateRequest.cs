namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class CloneTemplateRequest
{
    public Guid TemplateId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    
    // Optional customizations during cloning
    public string? CustomBaseLayer { get; set; }
    public string? CustomViewState { get; set; }
    public List<LayerCustomization>? LayerCustomizations { get; set; }
}

public class LayerCustomization
{
    public int OriginalLayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public string? CustomStyle { get; set; }
    public int? CustomZIndex { get; set; }
}
