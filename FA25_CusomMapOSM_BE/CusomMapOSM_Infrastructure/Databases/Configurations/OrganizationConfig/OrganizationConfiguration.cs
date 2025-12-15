using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.OrgId);
        builder.Property(o => o.OrgId).HasColumnName("org_id").IsRequired();

        builder.Property(o => o.OrgName).HasColumnName("org_name").HasMaxLength(255).IsRequired();
        builder.Property(o => o.Abbreviation).HasColumnName("abbreviation").HasMaxLength(50);
        builder.Property(o => o.Description).HasColumnName("description");
        builder.Property(o => o.LogoUrl).HasColumnName("logo_url");
        builder.Property(o => o.ContactEmail).HasColumnName("contact_email").HasMaxLength(255);
        builder.Property(o => o.ContactPhone).HasColumnName("contact_phone").HasMaxLength(50);
        builder.Property(o => o.Address).HasColumnName("address");
        builder.Property(o => o.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime");
        builder.Property(o => o.IsActive).HasColumnName("is_active");

        builder.HasOne(o => o.Owner)
               .WithMany()
               .HasForeignKey(o => o.OwnerUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
