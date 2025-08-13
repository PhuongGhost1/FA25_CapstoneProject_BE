using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Transaction;

public class CancelPaymentWithContextReqValidator : AbstractValidator<CancelPaymentWithContextReq>
{
    public CancelPaymentWithContextReqValidator()
    {
        RuleFor(x => x.PaymentGateway)
            .IsInEnum().WithMessage("Invalid payment gateway");

        // Gateway specific requirements
        When(x => x.PaymentGateway == PaymentGatewayEnum.PayPal, () =>
        {
            RuleFor(x => x.PaymentId).NotEmpty().WithMessage("PaymentId is required for PayPal");
            // PayerId and Token may be provided by gateway on cancel; keep optional unless your flow requires
        });

        When(x => x.PaymentGateway == PaymentGatewayEnum.Stripe, () =>
        {
            RuleFor(x => x.PaymentIntentId).NotEmpty().WithMessage("PaymentIntentId is required for Stripe");
            // ClientSecret may not be present during cancel redirect; keep optional unless enforced
        });
    }
}


