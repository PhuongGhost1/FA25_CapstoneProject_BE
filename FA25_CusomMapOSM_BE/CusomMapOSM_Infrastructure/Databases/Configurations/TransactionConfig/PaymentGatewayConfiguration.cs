using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
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
            new PaymentGateway { GatewayId = SeedDataConstants.VnPayPaymentGatewayId, Name = PaymentGatewayEnum.VNPay.ToString() },
            new PaymentGateway { GatewayId = SeedDataConstants.PayPalPaymentGatewayId, Name = PaymentGatewayEnum.PayPal.ToString() },
            new PaymentGateway { GatewayId = SeedDataConstants.StripePaymentGatewayId, Name = PaymentGatewayEnum.Stripe.ToString() },
            new PaymentGateway { GatewayId = SeedDataConstants.BankTransferPaymentGatewayId, Name = PaymentGatewayEnum.BankTransfer.ToString() }
        );
    }
}
