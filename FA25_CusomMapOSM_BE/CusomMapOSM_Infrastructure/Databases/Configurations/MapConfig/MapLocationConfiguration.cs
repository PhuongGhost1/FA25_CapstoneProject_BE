using CusomMapOSM_Domain.Entities.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapLocationConfiguration : IEntityTypeConfiguration<MapLocation>
{
    public void Configure(EntityTypeBuilder<MapLocation> builder)
    {
        builder.ToTable("map_locations");

        builder.HasKey(l => l.MapLocationId);

        builder.Property(l => l.MapLocationId)
            .HasColumnName("map_location_id")
            .IsRequired();

        builder.Property(l => l.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(l => l.SegmentId)
            .HasColumnName("segment_id");

        builder.Property(l => l.SegmentZoneId)
            .HasColumnName("segment_zone_id");

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(l => l.Subtitle)
            .HasColumnName("subtitle");

        builder.Property(l => l.LocationType)
            .HasColumnName("location_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.MarkerGeometry)
            .HasColumnName("marker_geometry")
            .HasColumnType("longtext");

        builder.Property(l => l.StoryContent)
            .HasColumnName("story_content")
            .HasColumnType("longtext");

        builder.Property(l => l.MediaResources)
            .HasColumnName("media_resources")
            .HasColumnType("json");

        builder.Property(l => l.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(l => l.HighlightOnEnter)
            .HasColumnName("highlight_on_enter")
            .HasDefaultValue(true);

        builder.Property(l => l.ShowTooltip)
            .HasColumnName("show_tooltip")
            .HasDefaultValue(true);

        builder.Property(l => l.TooltipContent)
            .HasColumnName("tooltip_content")
            .HasColumnType("longtext");

        builder.Property(l => l.EffectType)
            .HasColumnName("effect_type")
            .HasMaxLength(100);

        builder.Property(l => l.OpenSlideOnClick)
            .HasColumnName("open_slide_on_click")
            .HasDefaultValue(false);

        builder.Property(l => l.SlideContent)
            .HasColumnName("slide_content")
            .HasColumnType("longtext");

        builder.Property(l => l.LinkedLocationId)
            .HasColumnName("linked_location_id");

        builder.Property(l => l.PlayAudioOnClick)
            .HasColumnName("play_audio_on_click")
            .HasDefaultValue(false);

        builder.Property(l => l.AudioUrl)
            .HasColumnName("audio_url")
            .HasMaxLength(1024);

        builder.Property(l => l.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(1024);

        builder.Property(l => l.AssociatedLayerId)
            .HasColumnName("associated_layer_id");

        builder.Property(l => l.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(l => l.AnimationOverrides)
            .HasColumnName("animation_overrides")
            .HasColumnType("json");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.HasOne(l => l.Map)
            .WithMany()
            .HasForeignKey(l => l.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Segment)
            .WithMany()
            .HasForeignKey(l => l.SegmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.SegmentZone)
            .WithMany()
            .HasForeignKey(l => l.SegmentZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.AssociatedLayer)
            .WithMany()
            .HasForeignKey(l => l.AssociatedLayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.AnimationPreset)
            .WithMany()
            .HasForeignKey(l => l.AnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.LinkedLocation)
            .WithMany()
            .HasForeignKey(l => l.LinkedLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
