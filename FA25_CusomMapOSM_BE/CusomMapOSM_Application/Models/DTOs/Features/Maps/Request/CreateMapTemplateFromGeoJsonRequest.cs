using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public record CreateMapTemplateFromGeoJsonRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MapTemplateCategoryEnum Category { get; set; } = MapTemplateCategoryEnum.General;
    public bool IsPublic { get; set; } = false;
    public string LayerName { get; set; } = string.Empty;
    public string GeoJsonData { get; set; } = string.Empty;
    public string LayerStyle { get; set; } = string.Empty;
    public string DataBounds { get; set; } = string.Empty;
    public int FeatureCount { get; set; }
    public double DataSizeKB { get; set; }
}
