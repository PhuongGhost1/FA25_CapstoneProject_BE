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


