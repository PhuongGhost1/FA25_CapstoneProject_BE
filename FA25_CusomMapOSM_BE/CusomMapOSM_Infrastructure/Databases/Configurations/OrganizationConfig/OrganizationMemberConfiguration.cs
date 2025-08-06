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
              builder.Property(m => m.MembersRoleId).HasColumnName("role_id").IsRequired();
              builder.Property(m => m.InvitedBy).HasColumnName("invited_by");
              builder.Property(m => m.JoinedAt).HasColumnName("joined_at").HasColumnType("datetime").IsRequired();
              builder.Property(m => m.IsActive).HasColumnName("is_active");

              builder.HasOne(m => m.Organization)
                     .WithMany()
                     .HasForeignKey(m => m.OrgId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(m => m.User)
                     .WithMany()
                     .HasForeignKey(m => m.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(m => m.Role)
                     .WithMany()
                     .HasForeignKey(m => m.MembersRoleId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(m => m.Inviter)
                     .WithMany()
                     .HasForeignKey(m => m.InvitedBy)
                     .OnDelete(DeleteBehavior.Restrict);
       }
}
