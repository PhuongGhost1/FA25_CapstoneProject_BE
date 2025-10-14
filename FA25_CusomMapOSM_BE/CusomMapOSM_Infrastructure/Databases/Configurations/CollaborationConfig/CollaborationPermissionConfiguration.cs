using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Collaborations;
using CusomMapOSM_Domain.Entities.Collaborations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.CollaborationConfig;

internal class CollaborationPermissionConfiguration : IEntityTypeConfiguration<CollaborationPermission>
{
       public void Configure(EntityTypeBuilder<CollaborationPermission> builder)
       {
              builder.ToTable("collaboration_permissions");

              builder.HasKey(p => p.PermissionId);

              builder.Property(p => p.PermissionId)
                     .HasColumnName("permission_id")
                     .IsRequired();

              builder.Property(p => p.PermissionName)
                     .HasColumnName("permission_name")
                     .HasMaxLength(100);

              builder.Property(p => p.Description)
                     .HasColumnName("description")
                     .HasMaxLength(500);

              builder.Property(p => p.LevelOrder)
                     .HasColumnName("level_order")
                     .IsRequired();

              builder.Property(p => p.IsActive)
                     .HasColumnName("is_active")
                     .IsRequired();

              builder.Property(p => p.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              // Sample data based on URD collaboration requirements (view/edit/manage)
              builder.HasData(
                     new CollaborationPermission
                     {
                            PermissionId = SeedDataConstants.ViewPermissionId,
                            PermissionName = CollaborationPermissionEnum.View.ToString(),
                            Description = "Can view maps and layers",
                            LevelOrder = 1,
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     },
                     new CollaborationPermission
                     {
                            PermissionId = SeedDataConstants.EditPermissionId,
                            PermissionName = CollaborationPermissionEnum.Edit.ToString(),
                            Description = "Can edit maps and layers",
                            LevelOrder = 2,
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     },
                     new CollaborationPermission
                     {
                            PermissionId = SeedDataConstants.ManagePermissionId,
                            PermissionName = CollaborationPermissionEnum.Manage.ToString(),
                            Description = "Can manage maps, layers, and permissions",
                            LevelOrder = 3,
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     }
              );
       }
}
