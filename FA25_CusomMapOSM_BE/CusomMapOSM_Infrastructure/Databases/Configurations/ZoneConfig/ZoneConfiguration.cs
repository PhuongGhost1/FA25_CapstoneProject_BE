using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zones");

        builder.HasKey(z => z.ZoneId);

        builder.Property(z => z.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(z => z.ExternalId)
            .HasColumnName("external_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(z => z.ZoneCode)
            .HasColumnName("zone_code")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(z => z.AdminLevel)
            .HasColumnName("admin_level")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(z => z.ParentZoneId)
            .HasColumnName("parent_zone_id");

        builder.Property(z => z.Geometry)
            .HasColumnName("geometry")
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(z => z.SimplifiedGeometry)
            .HasColumnName("simplified_geometry")
            .HasColumnType("TEXT");

        builder.Property(z => z.Centroid)
            .HasColumnName("centroid")
            .HasColumnType("TEXT");

        builder.Property(z => z.BoundingBox)
            .HasColumnName("bounding_box")
            .HasColumnType("TEXT");

        builder.Property(z => z.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(z => z.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Relationships
        builder.HasOne(z => z.ParentZone)
            .WithMany()
            .HasForeignKey(z => z.ParentZoneId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}
