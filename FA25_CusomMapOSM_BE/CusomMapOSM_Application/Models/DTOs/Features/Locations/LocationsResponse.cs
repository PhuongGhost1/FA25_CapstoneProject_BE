using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Locations.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Locations;

public record LocationDto(
    Guid LocationId,
    Guid MapId,
    Guid? SegmentId,
    Guid? ZoneId,
    string Title,
    string? Subtitle,
    LocationType LocationType,
    string? MarkerGeometry,
    string? StoryContent,
    string? MediaResources,
    int DisplayOrder,
    
    // Icon configuration
    string? IconType,
    string? IconUrl,
    string? IconColor,
    int IconSize,
    
    bool HighlightOnEnter,
    bool ShowTooltip,
    string? TooltipContent,
    string? EffectType,
    bool OpenSlideOnClick,
    string? SlideContent,
    Guid? LinkedLocationId,
    bool PlayAudioOnClick,
    string? AudioUrl,
    string? ExternalUrl,
    Guid? AssociatedLayerId,
    Guid? AnimationPresetId,
    string? AnimationOverrides,
    
    // Animation effects
    int EntryDelayMs,
    int EntryDurationMs,
    int ExitDelayMs,
    int ExitDurationMs,
    string? EntryEffect,
    string? ExitEffect,
    
    bool IsVisible,
    int ZIndex,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
