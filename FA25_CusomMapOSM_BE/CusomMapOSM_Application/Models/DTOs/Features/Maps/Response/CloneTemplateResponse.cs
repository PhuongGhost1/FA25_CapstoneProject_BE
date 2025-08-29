namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class CloneTemplateResponse
{
    public Guid NewMapId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int LayersCloned { get; set; }
    public int FeaturesCloned { get; set; }
    public DateTime CreatedAt { get; set; }
}
