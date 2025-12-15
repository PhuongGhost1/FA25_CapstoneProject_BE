using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Application.Common.Mappers;

public static class SegmentMappings
{
    public static SegmentDto ToSegmentDto(
        this Segment segment,
        IReadOnlyCollection<SegmentZoneDto> zones,
        IReadOnlyCollection<SegmentLayerDto> layers,
        IReadOnlyCollection<LocationDto> locations)
        => new SegmentDto(
            segment.SegmentId,
            segment.MapId,
            segment.Name,
            segment.Description,
            segment.StoryContent,
            segment.DisplayOrder,
            segment.CameraState,
            segment.AutoAdvance,
            segment.DurationMs,
            segment.RequireUserAction,
            zones,
            layers,
            locations);

    public static SegmentDto ToSegmentDto(this Segment segment)
        => segment.ToSegmentDto(
            Array.Empty<SegmentZoneDto>(),
            Array.Empty<SegmentLayerDto>(),
            Array.Empty<LocationDto>());
}

