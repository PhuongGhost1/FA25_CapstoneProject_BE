using CusomMapOSM_Domain.Entities.Zones;
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
            .IsRequired();

        builder.Property(z => z.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(z => z.ZoneCode)
            .HasColumnName("zone_code")
            .HasMaxLength(50);

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(z => z.ZoneType)
            .HasColumnName("zone_type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(z => z.AdminLevel)
            .HasColumnName("admin_level");

        builder.Property(z => z.ParentZoneId)
            .HasColumnName("parent_zone_id");

        builder.Property(z => z.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(z => z.SimplifiedGeometry)
            .HasColumnName("simplified_geometry")
            .HasColumnType("text");

        builder.Property(z => z.Centroid)
            .HasColumnName("centroid")
            .HasColumnType("text");

        builder.Property(z => z.BoundingBox)
            .HasColumnName("bounding_box")
            .HasColumnType("text");

        builder.Property(z => z.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(z => z.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(z => z.LastSyncedAt)
            .HasColumnName("last_synced_at");

        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(z => z.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasOne(z => z.ParentZone)
            .WithMany()
            .HasForeignKey(z => z.ParentZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
