using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MembershipConfig;

internal class MembershipStatusConfiguration : IEntityTypeConfiguration<MembershipStatus>
{
    public void Configure(EntityTypeBuilder<MembershipStatus> builder)
    {
        builder.ToTable("membership_statuses");

        builder.HasKey(s => s.StatusId);

        builder.Property(s => s.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // Sample data for membership statuses
        builder.HasData(
            new MembershipStatus { StatusId = SeedDataConstants.ActiveMembershipStatusId, Name = "Active" },
            new MembershipStatus { StatusId = SeedDataConstants.ExpiredMembershipStatusId, Name = "Expired" },
            new MembershipStatus { StatusId = SeedDataConstants.SuspendedMembershipStatusId, Name = "Suspended" },
            new MembershipStatus { StatusId = SeedDataConstants.PendingPaymentMembershipStatusId, Name = "Pending Payment" },
            new MembershipStatus { StatusId = SeedDataConstants.CancelledMembershipStatusId, Name = "Cancelled" }
        );
    }
}
