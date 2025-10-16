using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
       public void Configure(EntityTypeBuilder<OrganizationMember> builder)
       {
              builder.ToTable("organization_members");

              builder.HasKey(m => m.MemberId);
              
              builder.Property(m => m.MemberId).HasColumnName("member_id").IsRequired();
              builder.Property(m => m.OrgId).HasColumnName("org_id").IsRequired();
              builder.Property(m => m.UserId).HasColumnName("user_id").IsRequired();
              
              builder.Property(m => m.Role)
                     .HasColumnName("role")
                     .HasConversion<string>()
                     .HasMaxLength(50)
                     .IsRequired();
              
              builder.Property(m => m.InvitationId).HasColumnName("invitation_id");
              builder.Property(m => m.InvitedBy).HasColumnName("invited_by");
              
              builder.Property(m => m.Status)
                     .HasColumnName("status")
                     .HasConversion<string>()
                     .HasMaxLength(20)
                     .IsRequired();
              
              builder.Property(m => m.JoinedAt).HasColumnName("joined_at").HasColumnType("datetime").IsRequired();
              builder.Property(m => m.LeftAt).HasColumnName("left_at").HasColumnType("datetime");
              builder.Property(m => m.LeaveReason).HasColumnName("leave_reason").HasMaxLength(500);

              // Relationships
              builder.HasOne(m => m.Organization)
                     .WithMany()
                     .HasForeignKey(m => m.OrgId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(m => m.User)
                     .WithMany()
                     .HasForeignKey(m => m.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(m => m.Inviter)
                     .WithMany()
                     .HasForeignKey(m => m.InvitedBy)
                     .OnDelete(DeleteBehavior.Restrict);
              
              builder.HasOne(m => m.Invitation)
                     .WithMany()
                     .HasForeignKey(m => m.InvitationId)
                     .OnDelete(DeleteBehavior.SetNull);

              // Indexes
              builder.HasIndex(m => new { m.OrgId, m.UserId }).IsUnique();
              builder.HasIndex(m => m.Status);
       }
}
