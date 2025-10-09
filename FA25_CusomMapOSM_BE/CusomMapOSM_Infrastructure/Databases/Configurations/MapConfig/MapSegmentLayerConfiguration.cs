using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapSegmentLayerConfiguration : IEntityTypeConfiguration<MapSegmentLayer>
{
    public void Configure(EntityTypeBuilder<MapSegmentLayer> builder)
    {
        builder.ToTable("map_segment_layers");

        builder.HasKey(sl => sl.SegmentLayerId);

        builder.Property(sl => sl.SegmentLayerId)
            .HasColumnName("segment_layer_id")
            .IsRequired();

        builder.Property(sl => sl.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(sl => sl.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        builder.Property(sl => sl.SegmentZoneId)
            .HasColumnName("segment_zone_id");

        builder.Property(sl => sl.ExpandToZone)
            .HasColumnName("expand_to_zone")
            .HasDefaultValue(true);

        builder.Property(sl => sl.HighlightZoneBoundary)
            .HasColumnName("highlight_zone_boundary")
            .HasDefaultValue(true);

        builder.Property(sl => sl.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(sl => sl.DelayMs)
            .HasColumnName("delay_ms")
            .HasDefaultValue(0);

        builder.Property(sl => sl.FadeInMs)
            .HasColumnName("fade_in_ms")
            .HasDefaultValue(400);

        builder.Property(sl => sl.FadeOutMs)
            .HasColumnName("fade_out_ms")
            .HasDefaultValue(400);

        builder.Property(sl => sl.StartOpacity)
            .HasColumnName("start_opacity")
            .HasDefaultValue(0.0);

        builder.Property(sl => sl.EndOpacity)
            .HasColumnName("end_opacity")
            .HasDefaultValue(1.0);

        builder.Property(sl => sl.Easing)
            .HasColumnName("easing")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(sl => sl.AutoPlayAnimation)
            .HasColumnName("auto_play_animation")
            .HasDefaultValue(true);

        builder.Property(sl => sl.RepeatCount)
            .HasColumnName("repeat_count")
            .HasDefaultValue(1);

        builder.Property(sl => sl.AnimationOverrides)
            .HasColumnName("animation_overrides")
            .HasColumnType("json");

        builder.Property(sl => sl.OverrideStyle)
            .HasColumnName("override_style")
            .HasColumnType("json");

        builder.Property(sl => sl.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json");

        builder.HasOne(sl => sl.Segment)
            .WithMany()
            .HasForeignKey(sl => sl.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.Layer)
            .WithMany()
            .HasForeignKey(sl => sl.LayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.SegmentZone)
            .WithMany()
            .HasForeignKey(sl => sl.SegmentZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sl => sl.AnimationPreset)
            .WithMany()
            .HasForeignKey(sl => sl.AnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
