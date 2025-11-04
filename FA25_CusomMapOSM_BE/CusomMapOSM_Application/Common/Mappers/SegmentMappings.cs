using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Application.Common.Mappers;

public static class SegmentMappings
{
    public static SegmentDto ToSegmentDto(
        this Segment segment,
        IReadOnlyCollection<SegmentZoneDto> zones,
        IReadOnlyCollection<SegmentLayerDto> layers,
        IReadOnlyCollection<PoiDto> locations)
        => new SegmentDto(
            segment.SegmentId,
            segment.MapId,
            segment.Name,
            segment.Description, // Summary mapped to Description
            segment.StoryContent,
            segment.DisplayOrder,
            false, // AutoFitBounds - not in entity, default to false
            null, // EntryAnimationPresetId - not in entity
            null, // ExitAnimationPresetId - not in entity
            null, // DefaultLayerAnimationPresetId - not in entity
            CusomMapOSM_Domain.Entities.Segments.Enums.SegmentPlaybackMode.Auto, // PlaybackMode - not in entity, default to Auto
            segment.CreatedAt,
            segment.UpdatedAt,
            zones,
            layers,
            locations);

    public static SegmentDto ToSegmentDto(this Segment segment)
        => segment.ToSegmentDto(
            Array.Empty<SegmentZoneDto>(),
            Array.Empty<SegmentLayerDto>(),
            Array.Empty<PoiDto>());
}

