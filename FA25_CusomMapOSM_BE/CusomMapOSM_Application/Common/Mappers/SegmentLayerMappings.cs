using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Application.Common.Mappers;

public static class SegmentLayerMappings
{
    public static SegmentLayerDto ToDto(this SegmentLayer segmentLayer)
        => new SegmentLayerDto(
            segmentLayer.SegmentLayerId,
            segmentLayer.SegmentId,
            segmentLayer.LayerId,
            segmentLayer.DisplayOrder,
            segmentLayer.IsVisible,
            segmentLayer.Opacity,
            segmentLayer.ZIndex,
            segmentLayer.EntryDelayMs,
            segmentLayer.EntryDurationMs,
            segmentLayer.ExitDelayMs,
            segmentLayer.ExitDurationMs,
            segmentLayer.EntryEffect,
            segmentLayer.ExitEffect,
            segmentLayer.StyleOverride,
            segmentLayer.CreatedAt,
            segmentLayer.UpdatedAt);
}
