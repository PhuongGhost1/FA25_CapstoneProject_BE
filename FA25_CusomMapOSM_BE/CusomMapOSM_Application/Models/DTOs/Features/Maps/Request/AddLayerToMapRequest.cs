using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class AddLayerToMapRequest
    {
        [Required]
        public Guid LayerId { get; set; }
        
        public string LayerName  { get; set; }
        
        public string LayerData  { get; set; }
        public string LayerTypeId{ get; set; }
        
        public bool IsVisible { get; set; } = true;
        
        public int ZIndex { get; set; } = 0;
        

    }
}
