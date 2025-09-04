using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class GetMapTemplatesResponse
    {
        public List<MapTemplateDTO> Templates { get; set; } = new();
    }
}
