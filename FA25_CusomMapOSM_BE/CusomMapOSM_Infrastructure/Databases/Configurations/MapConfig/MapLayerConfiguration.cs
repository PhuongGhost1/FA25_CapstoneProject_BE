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

              builder.Property(ml => ml.IsVisible)
                     .HasColumnName("is_visible");

              builder.Property(ml => ml.ZIndex)
                     .HasColumnName("z_index");

              builder.Property(ml => ml.LayerOrder)
                     .HasColumnName("layer_order");

              builder.Property(ml => ml.CustomStyle)
                     .HasColumnName("custom_style");

              builder.Property(ml => ml.FilterConfig)
                     .HasColumnName("filter_config");

              builder.Property(ml => ml.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(ml => ml.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");

              builder.HasOne(ml => ml.Map)
                     .WithMany()
                     .HasForeignKey(ml => ml.MapId);

              builder.HasOne(ml => ml.Layer)
                     .WithMany()
                     .HasForeignKey(ml => ml.LayerId);
       }
}
