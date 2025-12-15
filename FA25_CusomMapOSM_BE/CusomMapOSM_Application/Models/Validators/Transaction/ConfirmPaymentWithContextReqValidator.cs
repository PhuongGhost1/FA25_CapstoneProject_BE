using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Transaction;

public class ConfirmPaymentWithContextReqValidator : AbstractValidator<ConfirmPaymentWithContextReq>
{
    public ConfirmPaymentWithContextReqValidator()
    {
        RuleFor(x => x.PaymentGateway)
            .IsInEnum().WithMessage("Invalid payment gateway");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required")
            .Must(p => p.Equals("membership", StringComparison.OrdinalIgnoreCase) || 
                       p.Equals("upgrade", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Purpose must be 'membership' or 'upgrade'");

        RuleFor(x => x.TransactionId)
            .NotEqual(Guid.Empty).WithMessage("TransactionId is required");

        // Gateway specific requirements
        When(x => x.PaymentGateway == PaymentGatewayEnum.PayPal, () =>
        {
            RuleFor(x => x.PaymentId).NotEmpty().WithMessage("PaymentId is required for PayPal");
            RuleFor(x => x.PayerId).NotEmpty().WithMessage("PayerId is required for PayPal");
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required for PayPal");
        });

        When(x => x.PaymentGateway == PaymentGatewayEnum.Stripe, () =>
        {
            RuleFor(x => x.PaymentIntentId).NotEmpty().WithMessage("PaymentIntentId is required for Stripe");
            RuleFor(x => x.ClientSecret).NotEmpty().WithMessage("ClientSecret is required for Stripe");
        });

        // Business context requirements
        // Note: UserId, OrgId, and PlanId are optional here because they are stored in the transaction context
        // and retrieved from the transaction when processing the payment. The validator doesn't need to enforce
        // them in the request since they're already stored in the transaction.

    }
}


