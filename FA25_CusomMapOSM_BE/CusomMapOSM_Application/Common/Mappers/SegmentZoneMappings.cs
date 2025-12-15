using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Application.Common.Mappers;

public static class SegmentZoneMappings
{
    public static SegmentZoneDto ToDto(this SegmentZone segmentZone)
    {
        // Nếu Zone đã được Include, dùng thông tin từ Zone (master data)
        var zone = segmentZone.Zone;
        
        return new SegmentZoneDto(
            segmentZone.SegmentZoneId,
            segmentZone.SegmentId,
            segmentZone.ZoneId,
            segmentZone.DisplayOrder,
            segmentZone.IsVisible,
            segmentZone.ZIndex,
            segmentZone.HighlightBoundary,
            segmentZone.BoundaryColor,
            segmentZone.BoundaryWidth,
            segmentZone.FillZone,
            segmentZone.FillColor,
            segmentZone.FillOpacity,
            segmentZone.ShowLabel,
            segmentZone.LabelOverride,
            segmentZone.LabelStyle,
            segmentZone.EntryDelayMs,
            segmentZone.EntryDurationMs,
            segmentZone.ExitDelayMs,
            segmentZone.ExitDurationMs,
            segmentZone.EntryEffect,
            segmentZone.ExitEffect,
            segmentZone.FitBoundsOnEntry,
            segmentZone.CameraOverride,
            segmentZone.CreatedAt,
            segmentZone.UpdatedAt,
            // Include Zone summary if available
            zone != null ? zone.ToSummaryDto() : null
        );
    }
}
