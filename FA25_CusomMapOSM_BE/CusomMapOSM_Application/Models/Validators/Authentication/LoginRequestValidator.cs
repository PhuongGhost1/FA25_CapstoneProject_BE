using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Authentication;

public class LoginRequestValidator : AbstractValidator<LoginReqDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}