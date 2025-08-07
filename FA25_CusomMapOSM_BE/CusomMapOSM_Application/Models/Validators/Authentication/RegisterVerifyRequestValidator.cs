using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Authentication;

public class RegisterVerifyRequestValidator : AbstractValidator<RegisterVerifyReqDto>
{
    public RegisterVerifyRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
    }
}