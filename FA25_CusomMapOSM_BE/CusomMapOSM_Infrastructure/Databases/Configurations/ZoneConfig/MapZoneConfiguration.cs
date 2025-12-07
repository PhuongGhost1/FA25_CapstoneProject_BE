using CusomMapOSM_Domain.Entities.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class MapZoneConfiguration : IEntityTypeConfiguration<MapZone>
{
    public void Configure(EntityTypeBuilder<MapZone> builder)
    {
        builder.ToTable("map_zones");

        builder.HasKey(mz => mz.MapZoneId);

        builder.Property(mz => mz.MapZoneId)
            .HasColumnName("map_zone_id")
            .IsRequired();

        builder.Property(mz => mz.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(mz => mz.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        // Display settings
        builder.Property(mz => mz.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(mz => mz.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(mz => mz.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(0);

        // Highlight settings
        builder.Property(mz => mz.HighlightBoundary)
            .HasColumnName("highlight_boundary")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(mz => mz.BoundaryColor)
            .HasColumnName("boundary_color")
            .HasMaxLength(20);

        builder.Property(mz => mz.BoundaryWidth)
            .HasColumnName("boundary_width")
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(mz => mz.FillZone)
            .HasColumnName("fill_zone")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(mz => mz.FillColor)
            .HasColumnName("fill_color")
            .HasMaxLength(20);

        builder.Property(mz => mz.FillOpacity)
            .HasColumnName("fill_opacity")
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(0.3m);

        // Label settings
        builder.Property(mz => mz.ShowLabel)
            .HasColumnName("show_label")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(mz => mz.LabelOverride)
            .HasColumnName("label_override")
            .HasMaxLength(500);

        builder.Property(mz => mz.LabelStyle)
            .HasColumnName("label_style")
            .HasColumnType("TEXT");

        // Metadata
        builder.Property(mz => mz.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(mz => mz.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(mz => mz.Map)
            .WithMany()
            .HasForeignKey(mz => mz.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mz => mz.Zone)
            .WithMany()
            .HasForeignKey(mz => mz.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
