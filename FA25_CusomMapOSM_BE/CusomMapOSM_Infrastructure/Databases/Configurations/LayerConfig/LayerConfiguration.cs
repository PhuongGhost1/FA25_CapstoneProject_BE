using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.LayerConfig;

internal class LayerConfiguration : IEntityTypeConfiguration<Layer>
{
    public void Configure(EntityTypeBuilder<Layer> builder)
    {
        builder.ToTable("layers");

        builder.HasKey(l => l.LayerId);

        builder.Property(l => l.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        builder.Property(l => l.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.LayerName)
            .HasColumnName("layer_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.LayerType)
            .HasColumnName("layer_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.SourceType)
            .HasColumnName("source_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500);

        builder.Property(l => l.DataStoreKey)
            .HasColumnName("data_store_key")
            .HasMaxLength(256);

        builder.Property(l => l.LayerData)
            .HasColumnName("layer_data")
            .HasColumnType("TEXT");

        builder.Property(l => l.LayerStyle)
            .HasColumnName("layer_style")
            .HasColumnType("TEXT");
        
        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.FeatureCount)
            .HasColumnName("feature_count");

        builder.Property(l => l.DataSizeKB)
            .HasColumnName("data_size_kb")
            .HasColumnType("decimal(15,2)");

        builder.Property(l => l.DataBounds)
            .HasColumnName("data_bounds")
            .HasColumnType("TEXT");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(l => l.Map)
            .WithMany()
            .HasForeignKey(l => l.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
