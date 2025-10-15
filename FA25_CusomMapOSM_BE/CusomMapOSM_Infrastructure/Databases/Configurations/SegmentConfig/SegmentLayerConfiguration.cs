using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SegmentConfig;

internal class SegmentLayerConfiguration : IEntityTypeConfiguration<SegmentLayer>
{
    public void Configure(EntityTypeBuilder<SegmentLayer> builder)
    {
        builder.ToTable("segment_layers");

        builder.HasKey(sl => sl.SegmentLayerId);

        builder.Property(sl => sl.SegmentLayerId)
            .HasColumnName("segment_layer_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

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
            .IsRequired();

        builder.Property(sl => sl.HighlightZoneBoundary)
            .HasColumnName("highlight_zone_boundary")
            .IsRequired();

        builder.Property(sl => sl.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(sl => sl.DelayMs)
            .HasColumnName("delay_ms")
            .IsRequired();

        builder.Property(sl => sl.FadeInMs)
            .HasColumnName("fade_in_ms")
            .IsRequired();

        builder.Property(sl => sl.FadeOutMs)
            .HasColumnName("fade_out_ms")
            .IsRequired();

        builder.Property(sl => sl.StartOpacity)
            .HasColumnName("start_opacity")
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(sl => sl.EndOpacity)
            .HasColumnName("end_opacity")
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(sl => sl.Easing)
            .HasColumnName("easing")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sl => sl.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(sl => sl.AutoPlayAnimation)
            .HasColumnName("auto_play_animation")
            .IsRequired();

        builder.Property(sl => sl.RepeatCount)
            .HasColumnName("repeat_count")
            .IsRequired();

        builder.Property(sl => sl.AnimationOverrides)
            .HasColumnName("animation_overrides")
            .HasColumnType("TEXT");

        builder.Property(sl => sl.OverrideStyle)
            .HasColumnName("override_style")
            .HasColumnType("TEXT");

        builder.Property(sl => sl.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("TEXT");

        // Relationships
        builder.HasOne(sl => sl.Segment)
            .WithMany()
            .HasForeignKey(sl => sl.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.SegmentZone)
            .WithMany()
            .HasForeignKey(sl => sl.SegmentZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sl => sl.Layer)
            .WithMany()
            .HasForeignKey(sl => sl.LayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sl => sl.AnimationPreset)
            .WithMany()
            .HasForeignKey(sl => sl.AnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
