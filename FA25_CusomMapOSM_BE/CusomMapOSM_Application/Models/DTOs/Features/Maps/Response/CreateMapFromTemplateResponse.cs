using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class CreateMapFromTemplateResponse
    {
        public Guid MapId { get; set; }
        public string MapName { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public int LayersCreated { get; set; }
        public int ImagesCreated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = "Map created successfully from template";
    }
}
