using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class CreateMapFromTemplateRequest
    {
        [Required]
        public Guid TemplateId { get; set; }

        [StringLength(100)]
        public string? CustomName { get; set; }

        [StringLength(500)]
        public string? CustomDescription { get; set; }

        public bool IsPublic { get; set; } = false;

        public double? CustomInitialLatitude { get; set; }

        public double? CustomInitialLongitude { get; set; }

        [Range(1, 20)]
        public int? CustomInitialZoom { get; set; }
    }
}
