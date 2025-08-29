using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class ShareMapRequest
    {
        [Required]
        public Guid MapId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Permission { get; set; } = "Viewer"; // Owner, Editor, Viewer
    }
}
