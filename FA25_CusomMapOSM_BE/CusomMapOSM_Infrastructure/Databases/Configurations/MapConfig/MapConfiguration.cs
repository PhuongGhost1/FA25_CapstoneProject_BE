using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapConfiguration : IEntityTypeConfiguration<Map>
{
       public void Configure(EntityTypeBuilder<Map> builder)
       {
              builder.ToTable("maps");

              builder.HasKey(m => m.MapId);

              builder.Property(m => m.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(m => m.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(m => m.OrgId)
                     .HasColumnName("org_id")
                     .IsRequired();

              builder.Property(m => m.MapName)
                     .HasColumnName("map_name")
                     .HasMaxLength(255);

              builder.Property(m => m.Description)
                     .HasColumnName("description");

              builder.Property(m => m.GeographicBounds)
                     .HasColumnName("geographic_bounds");

              builder.Property(m => m.MapConfig)
                     .HasColumnName("map_config");

              builder.Property(m => m.BaseLayer)
                     .HasColumnName("base_layer")
                     .HasMaxLength(100)
                     .HasDefaultValue("osm");

              builder.Property(m => m.ViewState)
                     .HasColumnName("view_state");

              builder.Property(m => m.PreviewImage)
                     .HasColumnName("preview_image");

              builder.Property(m => m.IsPublic)
                     .HasColumnName("is_public")
                     .HasDefaultValue(false);

              builder.Property(m => m.IsActive)
                     .HasColumnName("is_active")
                     .HasDefaultValue(true);

              builder.Property(m => m.TemplateId)
                     .HasColumnName("template_id");

              builder.Property(m => m.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(m => m.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");

              builder.HasOne(m => m.User)
                     .WithMany()
                     .HasForeignKey(m => m.UserId);

              builder.HasOne(m => m.Organization)
                     .WithMany()
                     .HasForeignKey(m => m.OrgId);

              builder.HasOne(m => m.Template)
                     .WithMany()
                     .HasForeignKey(m => m.TemplateId);
       }
}
