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
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public string? QrCode { get; set; }
    public string? OrderCode { get; set; }
}

public record PurchaseAddonRequest
{
    public required Guid UserId { get; set; }
    public required Guid OrgId { get; set; }
    public required string AddonKey { get; set; }
    public required int Quantity { get; set; }
    public required PaymentGatewayEnum PaymentMethod { get; set; }
    public bool EffectiveImmediately { get; set; } = true;
}

public record PurchaseAddonResponse
{
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string TransactionId { get; set; }
    public required string PaymentUrl { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public string? QrCode { get; set; }
    public string? OrderCode { get; set; }
}

public record PaymentConfirmationRequest
{
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string TransactionId { get; set; }
    public required PaymentGatewayEnum PaymentMethod { get; set; }
    public required string Status { get; set; } // "success", "failed", "cancelled"
    public string? GatewayResponse { get; set; }
}

public record PaymentConfirmationResponse
{
    public required string TransactionId { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
    public bool MembershipUpdated { get; set; }
    public bool AccessToolsGranted { get; set; }
    public bool NotificationSent { get; set; }
}
