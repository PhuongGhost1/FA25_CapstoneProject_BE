using System;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Zones;

namespace CusomMapOSM_Domain.Entities.Locations;

public class Location
{
    public Guid LocationId { get; set; }
    public Guid MapId { get; set; }
    public Guid? SegmentId { get; set; }
    public Guid? ZoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public LocationType LocationType { get; set; } = LocationType.PointOfInterest;
    public string? MarkerGeometry { get; set; }
    public string? StoryContent { get; set; }
    public string? MediaResources { get; set; }
    public int DisplayOrder { get; set; }
    public bool HighlightOnEnter { get; set; } = true;
    public bool ShowTooltip { get; set; } = true;
    public string? TooltipContent { get; set; }
    public string? EffectType { get; set; }
    public bool OpenSlideOnClick { get; set; } = false;
    public string? SlideContent { get; set; }
    public Guid? LinkedLocationId { get; set; }
    public bool PlayAudioOnClick { get; set; } = false;
    public string? AudioUrl { get; set; }
    public string? ExternalUrl { get; set; }
    public Guid? AssociatedLayerId { get; set; }
    public Guid? AnimationPresetId { get; set; }
    public string? AnimationOverrides { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Map? Map { get; set; }
    public Segment? Segment { get; set; }
    public Zone? Zone { get; set; }
    public Layer? AssociatedLayer { get; set; }
    public LayerAnimationPreset? AnimationPreset { get; set; }
    public Location? LinkedLocation { get; set; }
}
