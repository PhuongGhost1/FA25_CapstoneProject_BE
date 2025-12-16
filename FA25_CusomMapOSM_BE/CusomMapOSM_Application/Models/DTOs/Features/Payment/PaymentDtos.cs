using CusomMapOSM_Domain.Entities.Transactions.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Payment;

public record SubscribeRequest
{
    public required Guid UserId { get; set; }
    public required Guid OrgId { get; set; }
    public required int PlanId { get; set; }
    public required PaymentGatewayEnum PaymentMethod { get; set; } // "payos", "stripe", "vnpay"
    public bool AutoRenew { get; set; } = false;
}

public record SubscribeResponse
{
    public required string TransactionId { get; set; }
    public required string PaymentUrl { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public string? QrCode { get; set; }
    public string? OrderCode { get; set; }
}

public record UpgradeRequest
{
    public required Guid UserId { get; set; }
    public required Guid OrgId { get; set; }
    public required int NewPlanId { get; set; }
    public required PaymentGatewayEnum PaymentMethod { get; set; }
    public bool AutoRenew { get; set; } = false;
}

public record UpgradeResponse
{
    public required string TransactionId { get; set; }
    public required string PaymentUrl { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public decimal? ProRatedAmount { get; set; }
    public decimal UnusedCredit { get; set; }
    public decimal ProratedNewPlanCost { get; set; }
    public int DaysRemaining { get; set; }
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public string? QrCode { get; set; }
    public string? OrderCode { get; set; }
}


public record PaymentConfirmationRequest
{
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string Purpose { get; set; }
    public required string TransactionId { get; set; }
    public required string Status { get; set; } // "success", "failed", "cancelled"
    public required string PaymentId { get; set; } // Stripe and PayOS specific
    public string? OrderCode { get; set; } // PayOS specific
}

public record PaymentConfirmationResponse
{
    public required string TransactionId { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public bool MembershipUpdated { get; set; }
    public bool NotificationSent { get; set; }
    public int AutoCancelledTransactions { get; set; }
}

public record RetryPaymentRequest
{
    public Guid TransactionId { get; init; }
}

public record RetryPaymentResponse
{
    public Guid TransactionId { get; init; }
    public string PaymentUrl { get; init; } = string.Empty;
    public string Status { get; init; } = "pending";
    public string? Message { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsNewUrl { get; init; }
}

public record PendingTransactionDto
{
    public required string TransactionId { get; set; }
    public required int PlanId { get; set; }
    public required string PlanName { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required DateTime CreatedAt { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ExpiresInMinutes { get; set; }
    public required string Description { get; set; }
}

public record PendingPaymentCheckResponse
{
    public required bool HasPending { get; set; }
    public PendingTransactionDto? Transaction { get; set; }
}

public record CancelPaymentRequest
{
    public required string Reason { get; set; }
    public string? Notes { get; set; }
}