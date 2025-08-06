using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationLocationConfiguration : IEntityTypeConfiguration<OrganizationLocation>
{
    public void Configure(EntityTypeBuilder<OrganizationLocation> builder)
    {
        builder.ToTable("organization_locations");

        builder.HasKey(l => l.LocationId);
        builder.Property(l => l.LocationId).HasColumnName("location_id").IsRequired().ValueGeneratedOnAdd();

        builder.Property(l => l.OrgId).HasColumnName("org_id").IsRequired();
        builder.Property(l => l.LocationName).HasColumnName("location_name").HasMaxLength(255).IsRequired();
        builder.Property(l => l.Address).HasColumnName("address");
        builder.Property(l => l.Latitude).HasColumnName("latitude").HasColumnType("decimal(10,6)");
        builder.Property(l => l.Longitude).HasColumnName("longitude").HasColumnType("decimal(10,6)");
        builder.Property(l => l.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(l => l.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(l => l.Website).HasColumnName("website");
        builder.Property(l => l.OperatingHours).HasColumnName("operating_hours");
        builder.Property(l => l.Services).HasColumnName("services");
        builder.Property(l => l.Categories).HasColumnName("categories");
        builder.Property(l => l.Amenities).HasColumnName("amenities");
        builder.Property(l => l.Photos).HasColumnName("photos");
        builder.Property(l => l.SocialMedia).HasColumnName("social_media");
        builder.Property(l => l.OrganizationLocationsStatusId).HasColumnName("status_id");
        builder.Property(l => l.Verified).HasColumnName("verified");
        builder.Property(l => l.LastVerifiedAt).HasColumnName("last_verified_at");
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime");

        builder.HasOne(l => l.Organization)
               .WithMany()
               .HasForeignKey(l => l.OrgId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Status)
               .WithMany()
               .HasForeignKey(l => l.OrganizationLocationsStatusId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
