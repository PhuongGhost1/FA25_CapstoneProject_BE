using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Tickets;
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
            new TicketStatus { StatusId = SeedDataConstants.OpenTicketStatusId, Name = "Open" },
            new TicketStatus { StatusId = SeedDataConstants.InProgressTicketStatusId, Name = "In Progress" },
            new TicketStatus { StatusId = SeedDataConstants.WaitingForCustomerTicketStatusId, Name = "Waiting for Customer" },
            new TicketStatus { StatusId = SeedDataConstants.ResolvedTicketStatusId, Name = "Resolved" },
            new TicketStatus { StatusId = SeedDataConstants.ClosedTicketStatusId, Name = "Closed" }
        );
    }
}
