using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Transaction;

public class ProcessPaymentReqValidator : AbstractValidator<ProcessPaymentReq>
{
    public ProcessPaymentReqValidator()
    {
        RuleFor(x => x.Total)
            .GreaterThan(0).WithMessage("Total must be greater than 0");

        RuleFor(x => x.PaymentGateway)
            .IsInEnum().WithMessage("Invalid payment gateway");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required")
            .Must(p => p.Equals("membership", StringComparison.OrdinalIgnoreCase) || p.Equals("addon", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Purpose must be either 'membership' or 'addon'");

        When(x => x.Purpose.Equals("addon", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.MembershipId).NotNull().WithMessage("MembershipId is required for addon purchases");
        });
    }
}


