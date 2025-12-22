using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class DuplicateMapRequest
    {
        [StringLength(100)]
        public string? CustomName { get; set; }
        
        [StringLength(500)]
        public string? CustomDescription { get; set; }
        
        public bool? IsPublic { get; set; }
        
        [Required]
        public Guid WorkspaceId { get; set; }
        
        public double? CustomInitialLatitude { get; set; }
        
        public double? CustomInitialLongitude { get; set; }
        
        public int? CustomInitialZoom { get; set; }
    }
}
