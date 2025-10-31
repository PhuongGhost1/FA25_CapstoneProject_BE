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
            segment.Summary,
            segment.StoryContent,
            segment.DisplayOrder,
            segment.AutoFitBounds,
            segment.EntryAnimationPresetId,
            segment.ExitAnimationPresetId,
            segment.DefaultLayerAnimationPresetId,
            segment.PlaybackMode,
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

