using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SegmentConfig;

internal class SegmentZoneConfiguration : IEntityTypeConfiguration<SegmentZone>
{
    public void Configure(EntityTypeBuilder<SegmentZone> builder)
    {
        builder.ToTable("segment_zones");

        builder.HasKey(sz => sz.SegmentZoneId);

        builder.Property(sz => sz.SegmentZoneId)
            .HasColumnName("segment_zone_id")
            .IsRequired();

        builder.Property(sz => sz.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(sz => sz.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        // Display settings
        builder.Property(sz => sz.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sz => sz.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sz => sz.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(0);

        // Highlight settings
        builder.Property(sz => sz.HighlightBoundary)
            .HasColumnName("highlight_boundary")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sz => sz.BoundaryColor)
            .HasColumnName("boundary_color")
            .HasMaxLength(20);

        builder.Property(sz => sz.BoundaryWidth)
            .HasColumnName("boundary_width")
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(sz => sz.FillZone)
            .HasColumnName("fill_zone")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sz => sz.FillColor)
            .HasColumnName("fill_color")
            .HasMaxLength(20);

        builder.Property(sz => sz.FillOpacity)
            .HasColumnName("fill_opacity")
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(0.3m);

        // Label settings
        builder.Property(sz => sz.ShowLabel)
            .HasColumnName("show_label")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sz => sz.LabelOverride)
            .HasColumnName("label_override")
            .HasMaxLength(500);

        builder.Property(sz => sz.LabelStyle)
            .HasColumnName("label_style")
            .HasColumnType("TEXT");

        // Entry Animation
        builder.Property(sz => sz.EntryDelayMs)
            .HasColumnName("entry_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sz => sz.EntryDurationMs)
            .HasColumnName("entry_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(sz => sz.EntryEffect)
            .HasColumnName("entry_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Exit Animation
        builder.Property(sz => sz.ExitDelayMs)
            .HasColumnName("exit_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sz => sz.ExitDurationMs)
            .HasColumnName("exit_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(sz => sz.ExitEffect)
            .HasColumnName("exit_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Camera behavior
        builder.Property(sz => sz.FitBoundsOnEntry)
            .HasColumnName("fit_bounds_on_entry")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sz => sz.CameraOverride)
            .HasColumnName("camera_override")
            .HasColumnType("TEXT");

        // Metadata
        builder.Property(sz => sz.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sz => sz.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(sz => sz.Segment)
            .WithMany()
            .HasForeignKey(sz => sz.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sz => sz.Zone)
            .WithMany()
            .HasForeignKey(sz => sz.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
