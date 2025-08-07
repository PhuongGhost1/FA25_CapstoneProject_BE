using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Tickets.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TicketConfig;

internal class TicketStatusConfiguration : IEntityTypeConfiguration<TicketStatus>
{
    public void Configure(EntityTypeBuilder<TicketStatus> builder)
    {
        builder.ToTable("ticket_statuses");

        builder.HasKey(x => x.StatusId);

        builder.Property(x => x.StatusId)
               .HasColumnName("status_id")
               .IsRequired();

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(100);

        // Sample data for support ticket statuses
        builder.HasData(
            new TicketStatus { StatusId = SeedDataConstants.OpenTicketStatusId, Name = TicketStatusEnum.Open.ToString() },
            new TicketStatus { StatusId = SeedDataConstants.InProgressTicketStatusId, Name = TicketStatusEnum.InProgress.ToString() },
            new TicketStatus { StatusId = SeedDataConstants.WaitingForCustomerTicketStatusId, Name = TicketStatusEnum.WaitingForCustomer.ToString() },
            new TicketStatus { StatusId = SeedDataConstants.ResolvedTicketStatusId, Name = TicketStatusEnum.Resolved.ToString() },
            new TicketStatus { StatusId = SeedDataConstants.ClosedTicketStatusId, Name = TicketStatusEnum.Closed.ToString() }
        );
    }
}
