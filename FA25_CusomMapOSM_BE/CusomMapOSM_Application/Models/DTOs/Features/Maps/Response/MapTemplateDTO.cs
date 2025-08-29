using System;
using System.Collections.Generic;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class MapTemplateDTO
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string PreviewImage { get; set; }
        public MapTemplateCategoryEnum Category { get; set; }
        public string BaseLayer { get; set; }
        public string? DefaultBounds { get; set; }
        public bool IsPublic { get; set; }
        public bool IsFeatured { get; set; }
        public int UsageCount { get; set; }
        public int TotalLayers { get; set; }
        public int TotalFeatures { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MapTemplateDetailDTO : MapTemplateDTO
    {
        public List<MapTemplateLayerDTO> Layers { get; set; } = new();
    }

    public class MapTemplateLayerDTO
    {
        public Guid LayerId { get; set; }
        public string LayerName { get; set; } = string.Empty;
        public int LayerTypeId { get; set; }
        public string LayerStyle { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public int ZIndex { get; set; }
        public int LayerOrder { get; set; }
        public int? FeatureCount { get; set; }
        public double? DataSizeKB { get; set; }
        public string? DataBounds { get; set; }
    }
}
