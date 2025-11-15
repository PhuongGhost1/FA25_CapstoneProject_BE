using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class GetOrganizationMapsResponse
    {
        public List<MapListItemDTO> Maps { get; set; } = new();
        public int TotalCount { get; set; }
        public string OrganizationName { get; set; }
    }
}
