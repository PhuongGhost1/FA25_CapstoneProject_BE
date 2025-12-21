using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Memberships;

namespace CusomMapOSM_Domain.Entities.Transactions;

public class Transactions
{
    public Guid TransactionId { get; set; }
    public required Guid PaymentGatewayId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public required decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? MembershipId { get; set; }
    public int? ExportId { get; set; }
    public required string Purpose { get; set; }
    public string? Content { get; set; } // Stores transaction content/details when transaction completes

    // Payment URL persistence fields
    public string? PaymentUrl { get; set; }
    public DateTime? PaymentUrlCreatedAt { get; set; }
    public DateTime? PaymentUrlExpiresAt { get; set; }
    public string? PaymentGatewayOrderCode { get; set; }

    // Cancellation tracking
    public string? CancellationReason { get; set; }

    // Navigation properties - DO NOT initialize to avoid EF Core creating new entities
    public PaymentGateway? PaymentGateway { get; set; }
    public Membership? Membership { get; set; }
    public Export? Export { get; set; }
}
