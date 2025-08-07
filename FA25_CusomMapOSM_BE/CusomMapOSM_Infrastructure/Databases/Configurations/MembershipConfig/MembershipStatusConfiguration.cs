using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
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
            new MembershipStatus { StatusId = SeedDataConstants.ActiveMembershipStatusId, Name = MembershipStatusEnum.Active.ToString() },
            new MembershipStatus { StatusId = SeedDataConstants.ExpiredMembershipStatusId, Name = MembershipStatusEnum.Expired.ToString() },
            new MembershipStatus { StatusId = SeedDataConstants.SuspendedMembershipStatusId, Name = MembershipStatusEnum.Suspended.ToString() },
            new MembershipStatus { StatusId = SeedDataConstants.PendingPaymentMembershipStatusId, Name = MembershipStatusEnum.PendingPayment.ToString() },
            new MembershipStatus { StatusId = SeedDataConstants.CancelledMembershipStatusId, Name = MembershipStatusEnum.Cancelled.ToString() }
        );
    }
}
