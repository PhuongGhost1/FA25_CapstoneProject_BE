using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.ToTable("organization_invitation");

        builder.HasKey(o => o.InvitationId);
        builder.Property(o => o.InvitationId).HasColumnName("invite_id").IsRequired();
        builder.Property(m => m.OrgId).HasColumnName("org_id").IsRequired();
        builder.Property(o => o.Email).HasColumnName("member_email").HasMaxLength(255).IsRequired();
        builder.Property(o => o.InvitedBy).HasColumnName("invited_by").HasMaxLength(50);
        builder.Property(m => m.MembersRoleId).HasColumnName("role_id").IsRequired();
        builder.Property(m => m.InvitedAt).HasColumnName("invited_at").HasColumnType("datetime").IsRequired();
        builder.Property(o => o.IsAccepted).HasColumnName("is_accepted").IsRequired();
        builder.Property(m => m.AcceptedAt).HasColumnName("accepted_at").HasColumnType("datetime").IsRequired(); 
        
        builder.HasOne(o => o.Inviter)
            .WithMany()
            .HasForeignKey(o => o.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Organization)
            .WithMany()
            .HasForeignKey(o => o.OrgId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(o => o.Role)
            .WithMany()
            .HasForeignKey(o => o.MembersRoleId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}