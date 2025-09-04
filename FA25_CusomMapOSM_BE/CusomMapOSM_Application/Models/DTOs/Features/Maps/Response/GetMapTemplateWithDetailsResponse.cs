using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class GetMapTemplateWithDetailsResponse
{
    public MapTemplateDTO Template { get; set; } = new();
    public List<MapLayerDTO> Layers { get; set; } = new();
    public List<MapImageDTO> Images { get; set; } = new();
}

public class MapLayerDTO
{
    public Guid MapLayerId { get; set; }
    public string LayerName { get; set; } = string.Empty;
    public int LayerTypeId { get; set; }
    public bool IsVisible { get; set; }
    public int ZIndex { get; set; }
    public int LayerOrder { get; set; }
    public string? LayerData { get; set; }
    public string? LayerStyle { get; set; }
    public string? CustomStyle { get; set; }
    public int? FeatureCount { get; set; }
    public double? DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
}


public class MapImageDTO
{
    public int MapImageId { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsVisible { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
