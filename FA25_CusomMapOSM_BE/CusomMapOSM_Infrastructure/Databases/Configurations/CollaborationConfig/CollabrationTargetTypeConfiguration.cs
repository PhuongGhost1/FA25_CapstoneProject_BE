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

internal class CollaborationTargetTypeConfiguration : IEntityTypeConfiguration<CollaborationTargetType>
{
       public void Configure(EntityTypeBuilder<CollaborationTargetType> builder)
       {
              builder.ToTable("collaboration_target_types");

              builder.HasKey(t => t.TargetTypeId);

              builder.Property(t => t.TargetTypeId)
                     .HasColumnName("target_type_id")
                     .IsRequired();

              builder.Property(t => t.TypeName)
                     .HasColumnName("type_name")
                     .HasMaxLength(100);

              builder.Property(t => t.Description)
                     .HasColumnName("description")
                     .HasMaxLength(500);

              builder.Property(t => t.IsActive)
                     .HasColumnName("is_active")
                     .IsRequired();

              builder.Property(t => t.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              // Sample data for collaboration target types
              builder.HasData(
                     new CollaborationTargetType
                     {
                            TargetTypeId = SeedDataConstants.MapTargetTypeId,
                            TypeName = CollaborationTargetTypeEnum.Map.ToString(),
                            Description = "Share entire maps with team members",
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     },
                     new CollaborationTargetType
                     {
                            TargetTypeId = SeedDataConstants.LayerTargetTypeId,
                            TypeName = CollaborationTargetTypeEnum.Layer.ToString(),
                            Description = "Share specific layers with team members",
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     },
                     new CollaborationTargetType
                     {
                            TargetTypeId = SeedDataConstants.OrganizationTargetTypeId,
                            TypeName = CollaborationTargetTypeEnum.Organization.ToString(),
                            Description = "Share organization resources",
                            IsActive = true,
                            CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
                     }
              );
       }
}
