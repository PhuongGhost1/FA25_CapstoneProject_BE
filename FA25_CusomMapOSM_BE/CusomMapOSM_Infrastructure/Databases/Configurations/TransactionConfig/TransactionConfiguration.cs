using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TransactionConfig;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transactions>
{
       public void Configure(EntityTypeBuilder<Transactions> builder)
       {
              builder.ToTable("transactions");

              builder.HasKey(t => t.TransactionId);

              builder.Property(t => t.TransactionId)
                     .HasColumnName("transaction_id")
                     .IsRequired();

              builder.Property(t => t.PaymentGatewayId)
                     .HasColumnName("payment_gateway_id")
                     .IsRequired();

              builder.Property(t => t.TransactionReference)
                     .HasColumnName("transaction_reference")
                     .HasMaxLength(100)
                     .IsRequired();

              builder.Property(t => t.Amount)
                     .HasColumnName("amount")
                     .HasPrecision(18, 2)
                     .IsRequired();

              builder.Property(t => t.Status)
                     .HasColumnName("status")
                     .HasMaxLength(50)
                     .HasDefaultValue("pending");

              builder.Property(t => t.TransactionDate)
                     .HasColumnName("transaction_date")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.Property(t => t.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.Property(t => t.MembershipId)
                     .HasColumnName("membership_id");

              builder.Property(t => t.ExportId)
                     .HasColumnName("export_id");

              builder.Property(t => t.Purpose)
                     .HasColumnName("purpose")
                     .HasColumnType("text")
                     .IsRequired();

              builder.Property(t => t.Content)
                     .HasColumnName("content")
                     .HasColumnType("text");

              // Relationships
              builder.HasOne(t => t.PaymentGateway)
                     .WithMany()
                     .HasForeignKey(t => t.PaymentGatewayId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(t => t.Membership)
                     .WithMany()
                     .HasForeignKey(t => t.MembershipId)
                     .OnDelete(DeleteBehavior.SetNull);

              builder.HasOne(t => t.Export)
                     .WithMany()
                     .HasForeignKey(t => t.ExportId)
                     .OnDelete(DeleteBehavior.SetNull);
       }
}
