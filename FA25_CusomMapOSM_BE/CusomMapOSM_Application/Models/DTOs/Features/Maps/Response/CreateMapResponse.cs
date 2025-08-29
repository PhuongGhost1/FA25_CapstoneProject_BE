using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class CreateMapResponse
    {
        public Guid MapId { get; set; }
        public string Message { get; set; } = "Map created successfully";
        public DateTime CreatedAt { get; set; }
    }
}
