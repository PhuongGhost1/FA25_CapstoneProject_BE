using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
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
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(sz => sz.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(sz => sz.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(sz => sz.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(sz => sz.ZoneType)
            .HasColumnName("zone_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sz => sz.ZoneGeometry)
            .HasColumnName("zone_geometry")
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(sz => sz.FocusCameraState)
            .HasColumnName("focus_camera_state")
            .HasColumnType("TEXT");

        builder.Property(sz => sz.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(sz => sz.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(sz => sz.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(sz => sz.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(sz => sz.Segment)
            .WithMany()
            .HasForeignKey(sz => sz.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
