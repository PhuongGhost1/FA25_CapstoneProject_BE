using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapTemplateConfiguration : IEntityTypeConfiguration<MapTemplate>
{
       public void Configure(EntityTypeBuilder<MapTemplate> builder)
       {
              builder.ToTable("map_templates");

              builder.HasKey(mt => mt.TemplateId);

              builder.Property(mt => mt.TemplateId)
                     .HasColumnName("template_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(mt => mt.TemplateName)
                     .HasColumnName("template_name")
                     .HasMaxLength(255)
                     .IsRequired();

              builder.Property(mt => mt.Description)
                     .HasColumnName("description");

              builder.Property(mt => mt.PreviewImage)
                     .HasColumnName("preview_image");

              builder.Property(mt => mt.DefaultBounds)
                     .HasColumnName("default_bounds");

              builder.Property(mt => mt.TemplateConfig)
                     .HasColumnName("template_config");

              builder.Property(mt => mt.BaseLayer)
                     .HasColumnName("base_layer")
                     .HasMaxLength(100)
                     .HasDefaultValue("osm");

              builder.Property(mt => mt.InitialLayers)
                     .HasColumnName("initial_layers");

              builder.Property(mt => mt.ViewState)
                     .HasColumnName("view_state");

              builder.Property(mt => mt.IsPublic)
                     .HasColumnName("is_public")
                     .HasDefaultValue(false);

              builder.Property(mt => mt.IsActive)
                     .HasColumnName("is_active")
                     .HasDefaultValue(true);

              builder.Property(mt => mt.IsFeatured)
                     .HasColumnName("is_featured")
                     .HasDefaultValue(false);

              builder.Property(mt => mt.UsageCount)
                     .HasColumnName("usage_count")
                     .HasDefaultValue(0);

              builder.Property(mt => mt.CreatedBy)
                     .HasColumnName("created_by");

              builder.Property(mt => mt.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(mt => mt.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");

              builder.HasOne(mt => mt.Creator)
                     .WithMany()
                     .HasForeignKey(mt => mt.CreatedBy);
       }
}
