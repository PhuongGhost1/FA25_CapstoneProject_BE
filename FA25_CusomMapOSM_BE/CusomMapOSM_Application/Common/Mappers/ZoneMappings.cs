using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Application.Common.Mappers;

public static class ZoneMappings
{
    // Zone master data to full DTO
    public static ZoneDto ToDto(this Zone zone)
        => new ZoneDto(
            zone.ZoneId,
            zone.ExternalId,
            zone.ZoneCode,
            zone.Name,
            zone.ZoneType,
            zone.AdminLevel,
            zone.ParentZoneId,
            zone.Geometry,
            zone.SimplifiedGeometry,
            zone.Centroid,
            zone.BoundingBox,
            zone.Description,
            zone.IsActive,
            zone.LastSyncedAt,
            zone.CreatedAt,
            zone.UpdatedAt);

    // Zone master data to summary DTO (lightweight)
    public static ZoneSummaryDto ToSummaryDto(this Zone zone)
        => new ZoneSummaryDto(
            zone.ZoneId,
            zone.Name,
            zone.ZoneCode,
            zone.ZoneType,
            zone.AdminLevel,
            zone.Centroid,
            zone.BoundingBox);
}

