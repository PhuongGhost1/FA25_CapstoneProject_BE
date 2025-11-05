using CusomMapOSM_Domain.Entities.Segments;
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
            .IsRequired();

        builder.Property(sl => sl.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(sl => sl.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        // Display settings
        builder.Property(sl => sl.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sl => sl.Opacity)
            .HasColumnName("opacity")
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(1.0m);

        builder.Property(sl => sl.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(0);

        // Entry Animation
        builder.Property(sl => sl.EntryDelayMs)
            .HasColumnName("entry_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.EntryDurationMs)
            .HasColumnName("entry_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(sl => sl.EntryEffect)
            .HasColumnName("entry_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Exit Animation
        builder.Property(sl => sl.ExitDelayMs)
            .HasColumnName("exit_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.ExitDurationMs)
            .HasColumnName("exit_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(sl => sl.ExitEffect)
            .HasColumnName("exit_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Style override
        builder.Property(sl => sl.StyleOverride)
            .HasColumnName("style_override")
            .HasColumnType("TEXT");

        // Metadata
        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sl => sl.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(sl => sl.Segment)
            .WithMany()
            .HasForeignKey(sl => sl.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.Layer)
            .WithMany()
            .HasForeignKey(sl => sl.LayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
