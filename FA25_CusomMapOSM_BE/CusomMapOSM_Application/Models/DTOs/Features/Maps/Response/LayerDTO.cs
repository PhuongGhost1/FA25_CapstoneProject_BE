using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class LayerDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int LayerTypeId { get; set; }
        public string LayerTypeName { get; set; }
        public string LayerTypeIcon { get; set; }
        
        public string SourceName { get; set; }
        
        public string FilePath { get; set; }
        public string LayerData { get; set; }
        public string LayerStyle { get; set; }
        
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        
        // MapLayer related properties
        public Guid MapLayerId { get; set; }
        public bool IsVisible { get; set; }
        public int ZIndex { get; set; }
        public int LayerOrder { get; set; }
        public string CustomStyle { get; set; }
        public string FilterConfig { get; set; }
    }
}
