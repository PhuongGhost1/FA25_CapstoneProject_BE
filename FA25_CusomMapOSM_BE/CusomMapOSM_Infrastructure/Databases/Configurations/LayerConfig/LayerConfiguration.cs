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
            .HasMaxLength(255)
            .HasColumnName("layer_name");

        builder.Property(l => l.LayerType)
            .HasColumnName("layer_type_id")
            .HasConversion<int>();

        builder.Property(l => l.SourceType)
            .HasColumnName("source_id")
            .HasConversion<int>();

        builder.Property(l => l.FilePath)
            .HasMaxLength(500)
            .HasColumnName("file_path");

        builder.Property(l => l.LayerData)
            .HasColumnType("longtext")
            .HasColumnName("layer_data");

        builder.Property(l => l.LayerStyle)
            .HasColumnType("longtext")
            .HasColumnName("layer_style");

        builder.Property(l => l.IsPublic)
            .HasColumnName("is_public");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .HasDefaultValue(true);

        builder.Property(l => l.ZIndex)
            .HasColumnName("z_index")
            .HasDefaultValue(0);

        builder.Property(l => l.LayerOrder)
            .HasColumnName("layer_order")
            .HasDefaultValue(0);

        builder.Property(l => l.CustomStyle)
            .HasColumnName("custom_style");

        builder.Property(l => l.FilterConfig)
            .HasColumnName("filter_config");
        
        builder.Property(l => l.FeatureCount)
            .HasColumnName("feature_count");

        builder.Property(l => l.DataSizeKB)
            .HasColumnName("data_size_kb")
            .HasColumnType("decimal(15,2)");

        builder.Property(l => l.DataBounds)
            .HasColumnName("data_bounds")
            .HasColumnType("text");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId);
        
        builder.HasOne(l => l.Map)
            .WithMany()    
            .HasForeignKey(l => l.MapId);
    }
}
