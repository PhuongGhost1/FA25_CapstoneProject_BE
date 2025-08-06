using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class AccountStatusConfiguration : IEntityTypeConfiguration<AccountStatus>
{
    public void Configure(EntityTypeBuilder<AccountStatus> builder)
    {
        builder.ToTable("account_statuses");

        builder.HasKey(s => s.StatusId);

        builder.Property(s => s.StatusId)
               .HasColumnName("status_id")
               .IsRequired();

        builder.Property(s => s.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(50);

        // Sample data for account statuses
        builder.HasData(
            new AccountStatus { StatusId = SeedDataConstants.ActiveStatusId, Name = "Active" },
            new AccountStatus { StatusId = SeedDataConstants.InactiveStatusId, Name = "Inactive" },
            new AccountStatus { StatusId = SeedDataConstants.SuspendedStatusId, Name = "Suspended" },
            new AccountStatus { StatusId = SeedDataConstants.PendingVerificationStatusId, Name = "Pending Verification" }
        );
    }
}
