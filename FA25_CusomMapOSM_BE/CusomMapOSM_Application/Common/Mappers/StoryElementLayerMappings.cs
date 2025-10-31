using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.StoryElement;

namespace CusomMapOSM_Application.Common.Mappers;

public static class StoryElementLayerMappings
{
    public static SegmentLayerDto ToSegmentLayerDto(this StoryElementLayer l)
        => new SegmentLayerDto(
            l.StoryElementLayerId,
            l.ElementId,
            l.LayerId,
            l.ZoneId,
            l.ExpandToZone,
            l.HighlightZoneBoundary,
            l.DisplayOrder,
            l.DelayMs,
            l.FadeInMs,
            l.FadeOutMs,
            l.StartOpacity,
            l.EndOpacity,
            l.Easing,
            l.AnimationPresetId,
            l.AutoPlayAnimation,
            l.RepeatCount,
            l.AnimationOverrides,
            l.StyleOverride,
            l.Metadata);

    public static StoryElementLayerDto ToStoryElementLayerDto(this StoryElementLayer sel)
        => new StoryElementLayerDto(
            sel.StoryElementLayerId,
            sel.ElementId,
            sel.ElementType,
            sel.LayerId,
            sel.ZoneId,
            sel.ExpandToZone,
            sel.HighlightZoneBoundary,
            sel.DisplayOrder,
            sel.DelayMs,
            sel.FadeInMs,
            sel.FadeOutMs,
            sel.StartOpacity,
            sel.EndOpacity,
            sel.Easing,
            sel.AnimationPresetId,
            sel.AutoPlayAnimation,
            sel.RepeatCount,
            sel.AnimationOverrides,
            sel.Metadata,
            sel.IsVisible,
            sel.Opacity,
            sel.DisplayMode,
            sel.StyleOverride,
            sel.CreatedAt,
            sel.UpdatedAt);
}

