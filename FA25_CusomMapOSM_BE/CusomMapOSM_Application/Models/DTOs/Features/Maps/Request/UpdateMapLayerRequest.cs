using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class UpdateMapLayerRequest
    {
        public string? LayerName { get; set; }

        public bool? IsVisible { get; set; }

        [Range(0, 1000)]
        public int? ZIndex { get; set; }

        public string? CustomStyle { get; set; }

        /// <summary>
        /// Whether features in this layer can have individual styles
        /// </summary>
        public bool? AllowIndividualFeatureStyles { get; set; }

        public string? FilterConfig { get; set; }
    }
}
