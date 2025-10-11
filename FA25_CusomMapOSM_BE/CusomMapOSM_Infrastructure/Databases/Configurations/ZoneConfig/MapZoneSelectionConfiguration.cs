using CusomMapOSM_Domain.Entities.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class MapZoneSelectionConfiguration : IEntityTypeConfiguration<MapZoneSelection>
{
    public void Configure(EntityTypeBuilder<MapZoneSelection> builder)
    {
        builder.ToTable("map_zone_selections");

        builder.HasKey(s => s.MapZoneSelectionId);

        builder.Property(s => s.MapZoneSelectionId)
            .HasColumnName("map_zone_selection_id")
            .IsRequired();

        builder.Property(s => s.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(s => s.SelectionGeometry)
            .HasColumnName("selection_geometry")
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(s => s.IncludedZoneIds)
            .HasColumnName("included_zone_ids")
            .HasColumnType("json");

        builder.Property(s => s.PersistResults)
            .HasColumnName("persist_results")
            .HasDefaultValue(false);

        builder.Property(s => s.Summary)
            .HasColumnName("summary");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(s => s.Map)
            .WithMany()
            .HasForeignKey(s => s.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Creator)
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
