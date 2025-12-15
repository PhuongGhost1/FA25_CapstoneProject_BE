using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Sessions;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty()
            .WithMessage("MapId is required");

        RuleFor(x => x.SessionName)
            .NotEmpty()
            .WithMessage("SessionName is required")
            .MaximumLength(200)
            .WithMessage("SessionName must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.MaxParticipants)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxParticipants must be 0 (unlimited) or a positive number");
    }
}

