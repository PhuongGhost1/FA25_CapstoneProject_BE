using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class CreateMapRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public bool IsPublic { get; set; } = false;
        
        [Required]
        public double InitialLatitude { get; set; }
        
        [Required]
        public double InitialLongitude { get; set; }
        
        [Required]
        [Range(1, 20)]
        public int InitialZoom { get; set; }
        
        [Required]
        public string BaseMapProvider { get; set; } = "OSM";
    }
}