using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Application.Common.Mappers;

public static class ZoneMappings
{
    public static SegmentZoneDto ToSegmentZoneDto(this Zone z)
        => new SegmentZoneDto(
            z.ZoneId,
            z.SegmentId,
            z.Name,
            z.Description,
            z.ZoneType,
            z.Geometry,
            z.FocusCameraState,
            z.DisplayOrder,
            z.IsPrimary,
            z.CreatedAt,
            z.UpdatedAt);
}

