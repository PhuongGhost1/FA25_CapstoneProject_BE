using System;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Infrastructure.Services.StoryMaps.Mongo;

[BsonIgnoreExtraElements]
internal class SegmentLocationBsonDocument
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("mapId")]
    public Guid MapId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("segmentId")]
    [BsonIgnoreIfNull]
    public Guid? SegmentId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("segmentZoneId")]
    [BsonIgnoreIfNull]
    public Guid? SegmentZoneId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("subtitle")]
    [BsonIgnoreIfNull]
    public string? Subtitle { get; set; }

    [BsonElement("locationType")]
    [BsonRepresentation(BsonType.String)]
    public SegmentLocationType LocationType { get; set; }

    [BsonElement("markerGeometry")]
    [BsonIgnoreIfNull]
    public string? MarkerGeometry { get; set; }

    [BsonElement("storyContent")]
    [BsonIgnoreIfNull]
    public string? StoryContent { get; set; }

    [BsonElement("mediaResources")]
    [BsonIgnoreIfNull]
    public string? MediaResources { get; set; }

    [BsonElement("displayOrder")]
    public int DisplayOrder { get; set; }

    [BsonElement("highlightOnEnter")]
    public bool HighlightOnEnter { get; set; }

    [BsonElement("showTooltip")]
    public bool ShowTooltip { get; set; }

    [BsonElement("tooltipContent")]
    [BsonIgnoreIfNull]
    public string? TooltipContent { get; set; }

    [BsonElement("effectType")]
    [BsonIgnoreIfNull]
    public string? EffectType { get; set; }

    [BsonElement("openSlideOnClick")]
    public bool OpenSlideOnClick { get; set; }

    [BsonElement("slideContent")]
    [BsonIgnoreIfNull]
    public string? SlideContent { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("linkedLocationId")]
    [BsonIgnoreIfNull]
    public Guid? LinkedLocationId { get; set; }

    [BsonElement("playAudioOnClick")]
    public bool PlayAudioOnClick { get; set; }

    [BsonElement("audioUrl")]
    [BsonIgnoreIfNull]
    public string? AudioUrl { get; set; }

    [BsonElement("externalUrl")]
    [BsonIgnoreIfNull]
    public string? ExternalUrl { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("associatedLayerId")]
    [BsonIgnoreIfNull]
    public Guid? AssociatedLayerId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonElement("animationPresetId")]
    [BsonIgnoreIfNull]
    public Guid? AnimationPresetId { get; set; }

    [BsonElement("animationOverrides")]
    [BsonIgnoreIfNull]
    public string? AnimationOverrides { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }

    public static SegmentLocationBsonDocument FromDomain(Location location)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        return new SegmentLocationBsonDocument
        {
            Id = location.LocationId == Guid.Empty ? Guid.NewGuid().ToString() : location.LocationId.ToString(),
            MapId = location.MapId,
            SegmentId = location.SegmentId,
            SegmentZoneId = location.SegmentZoneId,
            Title = location.Title,
            Subtitle = location.Subtitle,
            LocationType = location.LocationType,
            MarkerGeometry = location.MarkerGeometry,
            StoryContent = location.StoryContent,
            MediaResources = location.MediaResources,
            DisplayOrder = location.DisplayOrder,
            HighlightOnEnter = location.HighlightOnEnter,
            ShowTooltip = location.ShowTooltip,
            TooltipContent = location.TooltipContent,
            EffectType = location.EffectType,
            OpenSlideOnClick = location.OpenSlideOnClick,
            SlideContent = location.SlideContent,
            LinkedLocationId = location.LinkedLocationId,
            PlayAudioOnClick = location.PlayAudioOnClick,
            AudioUrl = location.AudioUrl,
            ExternalUrl = location.ExternalUrl,
            AssociatedLayerId = location.AssociatedLayerId,
            AnimationPresetId = location.AnimationPresetId,
            AnimationOverrides = location.AnimationOverrides,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }

    public Location ToDomain()
    {
        return new Location
        {
            LocationId = Guid.TryParse(Id, out var guid) ? guid : Guid.NewGuid(),
            MapId = MapId,
            SegmentId = SegmentId,
            SegmentZoneId = SegmentZoneId,
            Title = Title,
            Subtitle = Subtitle,
            LocationType = LocationType,
            MarkerGeometry = MarkerGeometry,
            StoryContent = StoryContent,
            MediaResources = MediaResources,
            DisplayOrder = DisplayOrder,
            HighlightOnEnter = HighlightOnEnter,
            ShowTooltip = ShowTooltip,
            TooltipContent = TooltipContent,
            EffectType = EffectType,
            OpenSlideOnClick = OpenSlideOnClick,
            SlideContent = SlideContent,
            LinkedLocationId = LinkedLocationId,
            PlayAudioOnClick = PlayAudioOnClick,
            AudioUrl = AudioUrl,
            ExternalUrl = ExternalUrl,
            AssociatedLayerId = AssociatedLayerId,
            AnimationPresetId = AnimationPresetId,
            AnimationOverrides = AnimationOverrides,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
