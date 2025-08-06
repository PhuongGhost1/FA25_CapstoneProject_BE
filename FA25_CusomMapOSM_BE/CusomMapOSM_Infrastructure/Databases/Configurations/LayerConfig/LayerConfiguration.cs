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

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.LayerName)
            .HasMaxLength(255)
            .HasColumnName("layer_name");

        builder.Property(l => l.LayerTypeId)
            .HasColumnName("layer_type_id");

        builder.Property(l => l.SourceId)
            .HasColumnName("source_id");

        builder.Property(l => l.FilePath)
            .HasMaxLength(500)
            .HasColumnName("file_path");

        builder.Property(l => l.LayerData)
            .HasColumnType("text") // For large GeoJSON or similar
            .HasColumnName("layer_data");

        builder.Property(l => l.LayerStyle)
            .HasColumnType("text")
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

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LayerType)
            .WithMany()
            .HasForeignKey(l => l.LayerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Source)
            .WithMany()
            .HasForeignKey(l => l.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
