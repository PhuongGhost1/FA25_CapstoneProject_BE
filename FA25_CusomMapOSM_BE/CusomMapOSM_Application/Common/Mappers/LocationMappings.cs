using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Locations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class LocationMappings
{
    public static PoiDto ToPoiDto(this Location l)
        => new PoiDto(
            l.LocationId,
            l.Segment?.MapId ?? Guid.Empty,
            l.SegmentId,
            null, // ZoneId - not in entity
            l.Title,
            l.Subtitle,
            l.LocationType,
            l.MarkerGeometry,
            null, // StoryContent - not in entity
            l.MediaUrls, // MediaResources mapped to MediaUrls
            l.DisplayOrder,
            false, // HighlightOnEnter - not in entity
            l.ShowTooltip,
            l.TooltipContent,
            null, // EffectType - not in entity
            l.OpenPopupOnClick, // OpenSlideOnClick mapped to OpenPopupOnClick
            l.PopupContent, // SlideContent mapped to PopupContent
            l.LinkedLocationId,
            l.PlayAudioOnClick,
            l.AudioUrl,
            l.ExternalUrl,
            null, // AssociatedLayerId - not in entity
            null, // AnimationPresetId - not in entity
            null, // AnimationOverrides - not in entity
            l.IsVisible,
            l.ZIndex,
            l.CreatedBy,
            l.CreatedAt,
            l.UpdatedAt);
}


