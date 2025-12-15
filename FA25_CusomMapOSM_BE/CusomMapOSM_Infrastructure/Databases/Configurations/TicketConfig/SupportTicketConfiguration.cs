using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TicketConfig;

internal class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
       public void Configure(EntityTypeBuilder<SupportTicket> builder)
       {
              builder.ToTable("support_tickets");

              builder.HasKey(x => x.TicketId);

              builder.Property(x => x.TicketId)
                     .HasColumnName("ticket_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(x => x.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(x => x.Subject)
                     .HasColumnName("subject")
                     .HasMaxLength(255);

              builder.Property(x => x.Message)
                     .HasColumnName("message")
                     .HasColumnType("text");

              builder.Property(x => x.Status)
                     .HasColumnName("status")
                     .HasConversion<int>()
                     .IsRequired();

              builder.Property(x => x.Priority)
                     .HasColumnName("priority")
                     .HasMaxLength(50)
                     .HasDefaultValue("low");

              builder.Property(x => x.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.Property(x => x.ResolvedAt)
                     .HasColumnName("resolved_at")
                     .HasColumnType("datetime");

              builder.HasOne(x => x.User)
                     .WithMany()
                     .HasForeignKey(x => x.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

       }
}
