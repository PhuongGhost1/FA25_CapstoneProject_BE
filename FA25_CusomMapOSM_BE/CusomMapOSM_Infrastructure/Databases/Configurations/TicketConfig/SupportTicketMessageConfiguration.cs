using CusomMapOSM_Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TicketConfig;

internal class SupportTicketMessageConfiguration : IEntityTypeConfiguration<SupportTicketMessage>
{
    public void Configure(EntityTypeBuilder<SupportTicketMessage> builder)
    {
        builder.ToTable("support_ticket_messages");

        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.MessageId)
               .HasColumnName("message_id")
               .IsRequired();

        builder.Property(m => m.TicketId)
               .HasColumnName("ticket_id")
               .IsRequired();

        builder.Property(m => m.Message)
               .HasColumnName("message")
               .HasMaxLength(4000);

        builder.Property(m => m.IsFromUser)
               .HasColumnName("is_from_user")
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("datetime")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign key relationship
        builder.HasOne(m => m.SupportTicket)
               .WithMany()
               .HasForeignKey(m => m.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(m => m.TicketId)
               .HasDatabaseName("IX_support_ticket_messages_ticket_id");

        builder.HasIndex(m => m.CreatedAt)
               .HasDatabaseName("IX_support_ticket_messages_created_at");
    }
}
