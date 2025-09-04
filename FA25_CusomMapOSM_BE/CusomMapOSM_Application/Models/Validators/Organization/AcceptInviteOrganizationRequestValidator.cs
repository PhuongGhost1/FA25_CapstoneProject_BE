using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Organization;

public class AcceptInviteOrganizationRequestValidator : AbstractValidator<AcceptInviteOrganizationReqDto>
{
    public AcceptInviteOrganizationRequestValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEmpty()
            .WithMessage("Invitation ID is required");
    }
}
