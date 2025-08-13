using CusomMapOSM_Application.Models.DTOs.Features.Membership;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Membership;

public class ChangeSubscriptionPlanRequestValidator : AbstractValidator<ChangeSubscriptionPlanRequest>
{
    public ChangeSubscriptionPlanRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.OrgId)
            .NotEmpty()
            .WithMessage("Organization ID is required");

        RuleFor(x => x.NewPlanId)
            .GreaterThan(0)
            .WithMessage("New plan ID must be greater than 0");

        RuleFor(x => x.AutoRenew)
            .NotNull()
            .WithMessage("Auto-renew setting is required");
    }
}
