using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapLegendItemConfiguration : IEntityTypeConfiguration<MapLegendItem>
{
    public void Configure(EntityTypeBuilder<MapLegendItem> builder)
    {
        builder.ToTable("map_legend_items");

        builder.HasKey(li => li.LegendItemId);

        builder.Property(li => li.LegendItemId)
            .HasColumnName("legend_item_id")
            .IsRequired();

        builder.Property(li => li.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(li => li.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(li => li.Label)
            .HasMaxLength(100)
            .HasColumnName("label")
            .IsRequired();

        builder.Property(li => li.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(li => li.Emoji)
            .HasMaxLength(10)
            .HasColumnName("emoji")
            .HasDefaultValue("📍");

        builder.Property(li => li.IconUrl)
            .HasMaxLength(500)
            .HasColumnName("icon_url");

        builder.Property(li => li.Color)
            .HasMaxLength(20)
            .HasColumnName("color");

        builder.Property(li => li.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(li => li.IsVisible)
            .HasColumnName("is_visible")
            .HasDefaultValue(true);

        builder.Property(li => li.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(li => li.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Foreign key relationships
        builder.HasOne(li => li.Map)
            .WithMany()
            .HasForeignKey(li => li.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.Creator)
            .WithMany()
            .HasForeignKey(li => li.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for faster queries
        builder.HasIndex(li => li.MapId)
            .HasDatabaseName("IX_map_legend_items_map_id");

        builder.HasIndex(li => new { li.MapId, li.DisplayOrder })
            .HasDatabaseName("IX_map_legend_items_map_order");
    }
}
