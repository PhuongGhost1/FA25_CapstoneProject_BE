using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class AddLayerToMapRequest
    {
        [Required]
        public Guid LayerId { get; set; }

        public bool IsVisible { get; set; } = true;

        [Range(0, 1000)]
        public int ZIndex { get; set; } = 0;

        public string? CustomStyle { get; set; }

        public string? FilterConfig { get; set; }
    }
}
