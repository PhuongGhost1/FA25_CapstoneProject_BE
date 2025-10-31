using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Locations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class LocationMappings
{
    public static PoiDto ToPoiDto(this Location l)
        => new PoiDto(
            l.LocationId,
            l.MapId,
            l.SegmentId,
            l.ZoneId,
            l.Title,
            l.Subtitle,
            l.LocationType,
            l.MarkerGeometry,
            l.StoryContent,
            l.MediaResources,
            l.DisplayOrder,
            l.HighlightOnEnter,
            l.ShowTooltip,
            l.TooltipContent,
            l.EffectType,
            l.OpenSlideOnClick,
            l.SlideContent,
            l.LinkedLocationId,
            l.PlayAudioOnClick,
            l.AudioUrl,
            l.ExternalUrl,
            l.AssociatedLayerId,
            l.AnimationPresetId,
            l.AnimationOverrides,
            l.IsVisible,
            l.ZIndex,
            l.CreatedBy,
            l.CreatedAt,
            l.UpdatedAt);
}


