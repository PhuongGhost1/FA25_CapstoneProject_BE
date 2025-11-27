using CusomMapOSM_Domain.Entities.Layers.Enums;
using System;
using System.Text.Json;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class LayerDTO
    {
        public Guid Id { get; set; }
        public string LayerName { get; set; } = string.Empty;
        public LayerType LayerType { get; set; }
        public LayerSource SourceType { get; set; }
        public string? FilePath { get; set; }
        public JsonDocument? LayerData { get; set; }
        public JsonDocument? LayerStyle { get; set; }
        public bool IsVisible { get; set; }
        public int? FeatureCount { get; set; }
        public double? DataSizeKB { get; set; }
        public string? DataBounds { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
