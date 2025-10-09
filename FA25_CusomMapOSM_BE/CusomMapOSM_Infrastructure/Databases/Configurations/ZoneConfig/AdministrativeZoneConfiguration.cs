using CusomMapOSM_Domain.Entities.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class AdministrativeZoneConfiguration : IEntityTypeConfiguration<AdministrativeZone>
{
    public void Configure(EntityTypeBuilder<AdministrativeZone> builder)
    {
        builder.ToTable("administrative_zones");

        builder.HasKey(z => z.ZoneId);

        builder.Property(z => z.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        builder.Property(z => z.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(z => z.ZoneCode)
            .HasColumnName("zone_code")
            .HasMaxLength(50);

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(z => z.AdminLevel)
            .HasColumnName("admin_level")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.ParentZoneId)
            .HasColumnName("parent_zone_id");

        builder.Property(z => z.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(z => z.SimplifiedGeometry)
            .HasColumnName("simplified_geometry")
            .HasColumnType("longtext");

        builder.Property(z => z.Centroid)
            .HasColumnName("centroid")
            .HasColumnType("json");

        builder.Property(z => z.BoundingBox)
            .HasColumnName("bounding_box")
            .HasColumnType("json");

        builder.Property(z => z.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(z => z.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasOne(z => z.ParentZone)
            .WithMany()
            .HasForeignKey(z => z.ParentZoneId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
