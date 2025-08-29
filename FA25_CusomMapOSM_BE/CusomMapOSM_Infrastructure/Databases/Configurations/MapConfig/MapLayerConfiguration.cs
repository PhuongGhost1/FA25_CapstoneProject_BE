using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapLayerConfiguration : IEntityTypeConfiguration<MapLayer>
{
       public void Configure(EntityTypeBuilder<MapLayer> builder)
       {
              builder.ToTable("map_layers");

              builder.HasKey(ml => ml.MapLayerId);

              builder.Property(ml => ml.MapLayerId)
                     .HasColumnName("map_layer_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(ml => ml.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(ml => ml.LayerId)
                     .HasColumnName("layer_id")
                     .IsRequired();
              
              builder.Property(ml => ml.LayerName)
                     .HasColumnName("layer_name")
                     .HasMaxLength(255);

              builder.Property(ml => ml.LayerTypeId)
                     .HasColumnName("layer_type_id");

              builder.Property(ml => ml.SourceId)
                     .HasColumnName("source_id");

              builder.Property(ml => ml.LayerData)
                     .HasColumnName("layer_data")
                     .HasColumnType("longtext");

              builder.Property(ml => ml.LayerStyle)
                     .HasColumnName("layer_style")
                     .HasColumnType("text");
              
              builder.Property(ml => ml.IsVisible)
                     .HasColumnName("is_visible")
                     .HasDefaultValue(true);

              builder.Property(ml => ml.ZIndex)
                     .HasColumnName("z_index")
                     .HasDefaultValue(0);

              builder.Property(ml => ml.LayerOrder)
                     .HasColumnName("layer_order")
                     .HasDefaultValue(0);

              builder.Property(ml => ml.CustomStyle)
                     .HasColumnName("custom_style");

              builder.Property(ml => ml.FilterConfig)
                     .HasColumnName("filter_config");
              
              builder.Property(ml => ml.FeatureCount)
                     .HasColumnName("feature_count");

              builder.Property(ml => ml.DataSizeKB)
                     .HasColumnName("data_size_kb")
                     .HasColumnType("decimal(15,2)");

              builder.Property(ml => ml.DataBounds)
                     .HasColumnName("data_bounds")
                     .HasColumnType("text");

              builder.Property(ml => ml.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(ml => ml.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");
              
              builder.HasOne(ml => ml.Map)
                     .WithMany(m => m.MapLayers)
                     .HasForeignKey(ml => ml.MapId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(ml => ml.Layer)
                     .WithMany()
                     .HasForeignKey(ml => ml.LayerId);
       }
}
