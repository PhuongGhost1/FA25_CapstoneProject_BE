using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class UpdateMapLayerRequest
    {
        public bool? IsVisible { get; set; }

        [Range(0, 1000)]
        public int? ZIndex { get; set; }

        public string? CustomStyle { get; set; }

        public string? FilterConfig { get; set; }
    }
}
