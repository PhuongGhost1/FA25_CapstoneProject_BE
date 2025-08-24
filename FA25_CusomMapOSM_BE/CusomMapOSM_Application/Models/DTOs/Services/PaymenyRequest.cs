using CusomMapOSM_Domain.Entities.Transactions.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Services;

public record ProcessCreatePaymentReq
{
    public required decimal Total { get; set; }
    public required string ReturnUrl { get; set; }
    public required string CancelUrl { get; set; }
}

public record ProcessPaymentReq
{
    public required decimal Total { get; set; }
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string Purpose { get; set; } // "membership" or "addon"
    public Guid? UserId { get; set; }
    public Guid? OrgId { get; set; }
    public int? PlanId { get; set; }
    public bool AutoRenew { get; set; } = true;
    public string? AddonKey { get; set; }
    public int? Quantity { get; set; }
    public Guid? MembershipId { get; set; }
}

// PayPal specific payment request
public record PayPalConfirmPaymentReq
{
    public required string PaymentId { get; set; }
    public required string PayerId { get; set; }
    public required string Token { get; set; }
}

// Stripe specific payment request
public record StripeConfirmPaymentReq
{
    public required string PaymentIntentId { get; set; }
    public required string ClientSecret { get; set; }
}

// PayOS specific payment request
public record PayOSConfirmPaymentReq
{
    public required string PaymentId { get; set; }
    public required string OrderCode { get; set; }
    public required string Signature { get; set; }
}

// Generic payment confirmation request
public record ConfirmPaymentReq
{
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string PaymentId { get; set; }
    public string? PayerId { get; set; } // PayPal specific
    public string? Token { get; set; } // PayPal specific
    public string? PaymentIntentId { get; set; } // Stripe specific
    public string? ClientSecret { get; set; } // Stripe specific
    public string? OrderCode { get; set; } // PayOS specific
    public string? Signature { get; set; } // PayOS specific
}

public class ApprovalUrlResponse
{
    public required string ApprovalUrl { get; set; }
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string SessionId { get; set; } // For tracking the payment session
    public string? QrCode { get; set; } // PayOS specific - QR code for payment
    public string? OrderCode { get; set; } // PayOS specific - order code
}

public class ConfirmPaymentResponse
{
    public required string PaymentId { get; set; }
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public string? PayerId { get; set; } // PayPal specific
    public string? Token { get; set; } // PayPal specific
    public string? PaymentIntentId { get; set; } // Stripe specific
    public string? ClientSecret { get; set; } // Stripe specific
    public string? OrderCode { get; set; } // PayOS specific
    public string? Signature { get; set; } // PayOS specific
}

public record ConfirmPaymentWithContextReq
{
    public required PaymentGatewayEnum PaymentGateway { get; set; }
    public required string PaymentId { get; set; }
    public string? PayerId { get; set; } // PayPal specific
    public string? Token { get; set; } // PayPal specific
    public string? PaymentIntentId { get; set; } // Stripe specific
    public string? ClientSecret { get; set; } // Stripe specific
    public string? OrderCode { get; set; } // PayOS specific
    public string? Signature { get; set; } // PayOS specific

    // Business context
    public required string Purpose { get; set; } // e.g., "membership" or "addon"
    public Guid TransactionId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrgId { get; set; }
    public int? PlanId { get; set; }
    public bool AutoRenew { get; set; } = true;
    public string? AddonKey { get; set; }
    public int? Quantity { get; set; }
    public Guid? MembershipId { get; set; }
}

// PayOS API Response Models
public class PayOSPaymentResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public PayOSPaymentData? Data { get; set; }
}

public class PayOSPaymentData
{
    public string PaymentLinkId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
}

public class PayOSPaymentDetails
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public PayOSPaymentDetailData? Data { get; set; }
}

public class PayOSPaymentDetailData
{
    public string PaymentLinkId { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}