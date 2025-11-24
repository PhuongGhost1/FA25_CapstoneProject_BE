using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Domain.Entities.Locations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class LocationMappings
{
    public static LocationDto ToDto(this Location l)
        => new LocationDto(
            l.LocationId,
            l.MapId, // Use MapId directly instead of l.Segment?.MapId for better performance
            l.SegmentId,
            l.ZoneId,
            l.Title,
            l.Subtitle,
            l.LocationType,
            l.MarkerGeometry,
            null, // StoryContent - not in entity
            l.MediaUrls, // MediaResources mapped to MediaUrls
            l.DisplayOrder,
            
            // Icon configuration
            l.IconType,
            l.IconUrl,
            l.IconColor,
            l.IconSize,
            
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
            
            // Animation effects
            l.EntryDelayMs,
            l.EntryDurationMs,
            l.ExitDelayMs,
            l.ExitDurationMs,
            l.EntryEffect,
            l.ExitEffect,
            
            l.IsVisible,
            l.ZIndex,
            l.CreatedBy,
            l.CreatedAt,
            l.UpdatedAt);
}


