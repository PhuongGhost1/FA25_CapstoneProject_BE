using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public record CreateMapTemplateFromGeoJsonResponse
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MapTemplateCategoryEnum Category { get; set; }
    public bool IsPublic { get; set; }
    public int LayerCount { get; set; }
    public int TotalFeatures { get; set; }
    public DateTime CreatedAt { get; set; }
}
