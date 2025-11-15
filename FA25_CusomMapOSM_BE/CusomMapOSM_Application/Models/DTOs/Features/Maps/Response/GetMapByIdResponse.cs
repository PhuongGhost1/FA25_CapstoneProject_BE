using System;
using System.Collections.Generic;
using System.Text.Json;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class GetMapByIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? PreviewImage { get; set; }       
        public JsonDocument? DefaultBounds { get; set; }     
        public string BaseLayer { get; set; }
        public JsonDocument ViewState { get; set; }
        public bool IsPublic { get; set; }
        public MapStatusEnum Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<LayerDTO> Layers { get; set; }
    }
}
