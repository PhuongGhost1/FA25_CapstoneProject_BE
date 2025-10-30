using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class AddLayerToMapRequest
    {
        // Optional: If provided, will add existing layer to map
        // If not provided (Guid.Empty), will create new layer with LayerName and LayerData
        public Guid LayerId { get; set; } = Guid.Empty;
        
        public string? LayerName  { get; set; }
        
        public string? LayerData  { get; set; }
        public string? LayerTypeId{ get; set; }
        
        public string? LayerStyle { get; set; }
        
        public bool IsVisible { get; set; } = true;
        
        public int ZIndex { get; set; } = 0;

    }
}
