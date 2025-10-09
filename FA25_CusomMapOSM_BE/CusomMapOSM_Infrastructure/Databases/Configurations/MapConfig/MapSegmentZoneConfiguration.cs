using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapSegmentZoneConfiguration : IEntityTypeConfiguration<MapSegmentZone>
{
    public void Configure(EntityTypeBuilder<MapSegmentZone> builder)
    {
        builder.ToTable("map_segment_zones");

        builder.HasKey(z => z.SegmentZoneId);

        builder.Property(z => z.SegmentZoneId)
            .HasColumnName("segment_zone_id")
            .IsRequired();

        builder.Property(z => z.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(z => z.Description)
            .HasColumnName("description");

        builder.Property(z => z.ZoneType)
            .HasColumnName("zone_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.ZoneGeometry)
            .HasColumnName("zone_geometry")
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(z => z.FocusCameraState)
            .HasColumnName("focus_camera_state")
            .HasColumnType("json");

        builder.Property(z => z.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(z => z.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false);

        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(z => z.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.HasOne(z => z.Segment)
            .WithMany()
            .HasForeignKey(z => z.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
