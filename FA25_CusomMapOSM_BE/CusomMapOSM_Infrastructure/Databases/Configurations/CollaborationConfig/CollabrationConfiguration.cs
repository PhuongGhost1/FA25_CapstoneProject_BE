using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Collaborations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.CollaborationConfig;

internal class CollaborationConfiguration : IEntityTypeConfiguration<Collaboration>
{
       public void Configure(EntityTypeBuilder<Collaboration> builder)
       {
              builder.ToTable("collaborations");

              builder.HasKey(c => c.CollaborationId);

              builder.Property(c => c.CollaborationId)
                     .HasColumnName("collaboration_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(c => c.TargetTypeId)
                     .HasColumnName("target_type_id")
                     .IsRequired();

              builder.Property(c => c.TargetId)
                     .HasColumnName("target_id")
                     .HasMaxLength(100)
                     .IsRequired();

              builder.Property(c => c.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(c => c.PermissionId)
                     .HasColumnName("permission_id")
                     .IsRequired();

              builder.Property(c => c.InvitedBy)
                     .HasColumnName("invited_by");

              builder.Property(c => c.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(c => c.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");

              // Relationships
              builder.HasOne(c => c.TargetType)
                     .WithMany()
                     .HasForeignKey(c => c.TargetTypeId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(c => c.User)
                     .WithMany()
                     .HasForeignKey(c => c.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(c => c.Permission)
                     .WithMany()
                     .HasForeignKey(c => c.PermissionId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(c => c.Inviter)
                     .WithMany()
                     .HasForeignKey(c => c.InvitedBy)
                     .OnDelete(DeleteBehavior.SetNull);
       }
}
