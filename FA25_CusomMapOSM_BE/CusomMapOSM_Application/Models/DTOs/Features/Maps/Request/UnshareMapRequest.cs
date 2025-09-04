using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request
{
    public class UnshareMapRequest
    {
        [Required]
        public Guid MapId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
