using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.NotificationConfig;

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Table name
        builder.ToTable("notifications");

        // Primary Key
        builder.HasKey(n => n.NotificationId);

        // Properties
        builder.Property(n => n.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasMaxLength(100);

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .HasMaxLength(1000);

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
