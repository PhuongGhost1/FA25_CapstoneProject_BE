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
        
        public string? DefaultBounds { get; set; }  // GeoJSON Polygon or null
        
        public string? ViewState { get; set; }       // JSON object {"center":[lat,lng],"zoom":zoom}
        
        public string? BaseMapProvider { get; set; } = "osm";

        public Guid? WorkspaceId { get; set; }
    }
}
