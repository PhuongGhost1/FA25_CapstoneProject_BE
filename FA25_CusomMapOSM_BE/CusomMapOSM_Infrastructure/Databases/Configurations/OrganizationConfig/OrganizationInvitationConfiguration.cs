using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.ToTable("organization_invitations");

        builder.HasKey(o => o.InvitationId);
        
        builder.Property(o => o.InvitationId).HasColumnName("invitation_id").IsRequired();
        builder.Property(o => o.OrgId).HasColumnName("org_id").IsRequired();
        builder.Property(o => o.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(o => o.InvitedBy).HasColumnName("invited_by").IsRequired();
        
        builder.Property(o => o.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(o => o.InvitationToken).HasColumnName("invitation_token").HasMaxLength(255);
        builder.Property(o => o.InvitedAt).HasColumnName("invited_at").HasColumnType("datetime").IsRequired();
        builder.Property(o => o.ExpiresAt).HasColumnName("expires_at").HasColumnType("datetime").IsRequired();
        builder.Property(o => o.RespondedAt).HasColumnName("responded_at").HasColumnType("datetime");
        builder.Property(o => o.Message).HasColumnName("message").HasMaxLength(500);
        
        // Relationships
        builder.HasOne(o => o.Inviter)
            .WithMany()
            .HasForeignKey(o => o.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(o => o.Organization)
            .WithMany()
            .HasForeignKey(o => o.OrgId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.Email);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.ExpiresAt);
    }
}