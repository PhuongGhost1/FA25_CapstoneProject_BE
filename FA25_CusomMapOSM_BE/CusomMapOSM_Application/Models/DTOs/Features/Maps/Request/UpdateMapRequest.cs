using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class UpdateMapRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsPublic { get; set; }

        public double? InitialLatitude { get; set; }

        public double? InitialLongitude { get; set; }

        [Range(1, 20)]
        public int? InitialZoom { get; set; }

        public string? BaseLayer { get; set; }

        public string? GeographicBounds { get; set; }

        public string? ViewState { get; set; }
    }
}
