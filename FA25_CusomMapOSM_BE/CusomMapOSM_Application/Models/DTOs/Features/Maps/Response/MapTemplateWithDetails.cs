using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapTemplateWithDetails
{
    public Map Map { get; set; } = new();
    public List<MapLayer> MapLayers { get; set; } = new();
    public List<MapImage> MapImages { get; set; } = new();
}
