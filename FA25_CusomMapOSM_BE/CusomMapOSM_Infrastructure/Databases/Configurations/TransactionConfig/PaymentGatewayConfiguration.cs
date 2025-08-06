using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TransactionConfig;

internal class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.ToTable("payment_gateways");

        builder.HasKey(pg => pg.GatewayId);

        builder.Property(pg => pg.GatewayId)
               .HasColumnName("gateway_id")
               .IsRequired();

        builder.Property(pg => pg.Name)
               .HasColumnName("name")
               .HasMaxLength(100)
               .IsRequired();

        // Sample data based on URD payment requirements
        builder.HasData(
            new PaymentGateway { GatewayId = SeedDataConstants.VnPayPaymentGatewayId, Name = "VNPay" },
            new PaymentGateway { GatewayId = SeedDataConstants.PayPalPaymentGatewayId, Name = "PayPal" },
            new PaymentGateway { GatewayId = SeedDataConstants.StripePaymentGatewayId, Name = "Stripe" },
            new PaymentGateway { GatewayId = SeedDataConstants.BankTransferPaymentGatewayId, Name = "Bank Transfer" }
        );
    }
}
