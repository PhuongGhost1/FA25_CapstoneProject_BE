using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationMemberTypeConfiguration : IEntityTypeConfiguration<OrganizationMemberType>
{
    public void Configure(EntityTypeBuilder<OrganizationMemberType> builder)
    {
        builder.ToTable("organization_member_types");

        builder.HasKey(r => r.TypeId);
        builder.Property(r => r.TypeId).HasColumnName("type_id").IsRequired();

        builder.Property(r => r.Name)
               .HasColumnName("name")
               .HasMaxLength(100)
               .IsRequired();

        // Sample data for organization member types
        builder.HasData(
            new OrganizationMemberType { TypeId = SeedDataConstants.OwnerOrganizationMemberTypeId, Name = "Owner" },
            new OrganizationMemberType { TypeId = SeedDataConstants.AdminOrganizationMemberTypeId, Name = "Admin" },
            new OrganizationMemberType { TypeId = SeedDataConstants.MemberOrganizationMemberTypeId, Name = "Member" },
            new OrganizationMemberType { TypeId = SeedDataConstants.ViewerOrganizationMemberTypeId, Name = "Viewer" }
        );
    }
}
