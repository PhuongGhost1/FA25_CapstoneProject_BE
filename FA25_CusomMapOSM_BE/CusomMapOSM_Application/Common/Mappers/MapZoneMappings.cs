using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Application.Common.Mappers;

public static class MapZoneMappings
{
    public static MapZoneDto ToDto(this MapZone mapZone)
    {
        var zone = mapZone.Zone;
        
        return new MapZoneDto(
            mapZone.MapZoneId,
            mapZone.MapId,
            mapZone.ZoneId,
            mapZone.DisplayOrder,
            mapZone.IsVisible,
            mapZone.ZIndex,
            mapZone.HighlightBoundary,
            mapZone.BoundaryColor,
            mapZone.BoundaryWidth,
            mapZone.FillZone,
            mapZone.FillColor,
            mapZone.FillOpacity,
            mapZone.ShowLabel,
            mapZone.LabelOverride,
            mapZone.LabelStyle,
            mapZone.CreatedAt,
            mapZone.UpdatedAt,
            zone != null ? zone.ToSummaryDto() : null
        );
    }
}
