using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.OrganizationConfig;

internal class OrganizationLocationStatusConfiguration : IEntityTypeConfiguration<OrganizationLocationStatus>
{
    public void Configure(EntityTypeBuilder<OrganizationLocationStatus> builder)
    {
        builder.ToTable("organization_location_statuses");

        builder.HasKey(s => s.StatusId);
        builder.Property(s => s.StatusId).HasColumnName("status_id").IsRequired();

        builder.Property(s => s.Name)
               .HasColumnName("name")
               .HasMaxLength(100)
               .IsRequired();

        // Sample data for organization location statuses
        builder.HasData(
            new OrganizationLocationStatus { StatusId = SeedDataConstants.ActiveOrganizationLocationStatusId, Name = OrganizationLocationStatusEnum.Active.ToString() },
            new OrganizationLocationStatus { StatusId = SeedDataConstants.InactiveOrganizationLocationStatusId, Name = OrganizationLocationStatusEnum.Inactive.ToString() },
            new OrganizationLocationStatus { StatusId = SeedDataConstants.UnderConstructionOrganizationLocationStatusId, Name = OrganizationLocationStatusEnum.UnderConstruction.ToString() },
            new OrganizationLocationStatus { StatusId = SeedDataConstants.TemporaryClosedOrganizationLocationStatusId, Name = OrganizationLocationStatusEnum.TemporaryClosed.ToString() }
        );
    }
}
