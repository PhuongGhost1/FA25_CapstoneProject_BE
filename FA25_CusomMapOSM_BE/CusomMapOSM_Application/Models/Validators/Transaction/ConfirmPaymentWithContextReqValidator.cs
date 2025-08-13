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
            .Must(p => p.Equals("membership", StringComparison.OrdinalIgnoreCase) || p.Equals("addon", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Purpose must be either 'membership' or 'addon'");

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
        When(x => x.Purpose.Equals("membership", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required for membership purchases");
            RuleFor(x => x.OrgId).NotNull().WithMessage("OrgId is required for membership purchases");
            RuleFor(x => x.PlanId).NotNull().WithMessage("PlanId is required for membership purchases");
        });

        When(x => x.Purpose.Equals("addon", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.MembershipId).NotNull().WithMessage("MembershipId is required for addon purchases");
            RuleFor(x => x.OrgId).NotNull().WithMessage("OrgId is required for addon purchases");
            RuleFor(x => x.AddonKey).NotEmpty().WithMessage("AddonKey is required for addon purchases");
            RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue).WithMessage("Quantity must be greater than 0 when specified");
        });
    }
}


