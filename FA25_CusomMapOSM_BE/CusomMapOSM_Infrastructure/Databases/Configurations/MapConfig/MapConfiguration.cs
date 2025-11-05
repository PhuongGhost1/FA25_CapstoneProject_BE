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

              builder.Property(m => m.WorkspaceId)
                     .HasColumnName("workspace_id");

              builder.Property(m => m.MapName)
                     .HasColumnName("map_name")
                     .HasMaxLength(255);

              builder.Property(m => m.Description)
                     .HasColumnName("description");
              
              builder.Property(m => m.IsTemplate)
                     .HasColumnName("is_template")
                     .HasDefaultValue(false);

              builder.Property(m => m.ParentMapId)
                     .HasColumnName("parent_map_id");

              builder.Property(m => m.Category)
                     .HasColumnName("category")
                     .HasMaxLength(50)
                     .HasConversion<string>();

              builder.Property(m => m.IsFeatured)
                     .HasColumnName("is_featured")
                     .HasDefaultValue(false);

              builder.Property(m => m.UsageCount)
                     .HasColumnName("usage_count")
                     .HasDefaultValue(0);
              
              builder.Property(m => m.DefaultBounds)
                     .HasColumnName("default_bounds");

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

              builder.Property(m => m.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.Property(m => m.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");
              
              
              builder.HasOne(m => m.User)
                     .WithMany()
                     .HasForeignKey(m => m.UserId);

              builder.HasOne(m => m.Workspace)
                     .WithMany()
                     .HasForeignKey(m => m.WorkspaceId)
                     .OnDelete(DeleteBehavior.SetNull);
              
              builder.HasOne(m => m.ParentMap)
                     .WithMany()
                     .HasForeignKey(m => m.ParentMapId)
                     .OnDelete(DeleteBehavior.SetNull);
       }
}
