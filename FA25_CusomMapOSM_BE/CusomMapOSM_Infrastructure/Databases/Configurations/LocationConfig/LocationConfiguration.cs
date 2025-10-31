using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.LocationConfig;

internal class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.LocationId);

        builder.Property(l => l.LocationId)
            .HasColumnName("location_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(l => l.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(l => l.SegmentId)
            .HasColumnName("segment_id");

        builder.Property(l => l.ZoneId)
            .HasColumnName("zone_id");

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(255);

        builder.Property(l => l.LocationType)
            .HasColumnName("location_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.MarkerGeometry)
            .HasColumnName("marker_geometry")
            .HasColumnType("TEXT");

        builder.Property(l => l.StoryContent)
            .HasColumnName("story_content")
            .HasColumnType("TEXT");

        builder.Property(l => l.MediaResources)
            .HasColumnName("media_resources")
            .HasColumnType("TEXT");

        builder.Property(l => l.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(l => l.HighlightOnEnter)
            .HasColumnName("highlight_on_enter")
            .IsRequired();

        builder.Property(l => l.ShowTooltip)
            .HasColumnName("show_tooltip")
            .IsRequired();

        builder.Property(l => l.TooltipContent)
            .HasColumnName("tooltip_content")
            .HasColumnType("TEXT");

        builder.Property(l => l.EffectType)
            .HasColumnName("effect_type")
            .HasMaxLength(100);

        builder.Property(l => l.OpenSlideOnClick)
            .HasColumnName("open_slide_on_click")
            .IsRequired();

        builder.Property(l => l.SlideContent)
            .HasColumnName("slide_content")
            .HasColumnType("TEXT");

        builder.Property(l => l.LinkedLocationId)
            .HasColumnName("linked_location_id");

        builder.Property(l => l.PlayAudioOnClick)
            .HasColumnName("play_audio_on_click")
            .IsRequired();

        builder.Property(l => l.AudioUrl)
            .HasColumnName("audio_url")
            .HasMaxLength(500);

        builder.Property(l => l.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(500);

        builder.Property(l => l.AssociatedLayerId)
            .HasColumnName("associated_layer_id");

        builder.Property(l => l.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(l => l.AnimationOverrides)
            .HasColumnName("animation_overrides")
            .HasColumnType("TEXT");

        // Enhanced interaction fields
        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired();

        builder.Property(l => l.ZIndex)
            .HasColumnName("z_index")
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(l => l.Map)
            .WithMany()
            .HasForeignKey(l => l.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Segment)
            .WithMany()
            .HasForeignKey(l => l.SegmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Zone)
            .WithMany()
            .HasForeignKey(l => l.ZoneId)
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

        builder.HasOne(l => l.Creator)
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
