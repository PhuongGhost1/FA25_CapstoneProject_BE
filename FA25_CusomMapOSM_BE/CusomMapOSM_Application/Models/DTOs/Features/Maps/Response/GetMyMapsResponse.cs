using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class GetMyMapsResponse
    {
        public List<MapDetailDTO> Maps { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
