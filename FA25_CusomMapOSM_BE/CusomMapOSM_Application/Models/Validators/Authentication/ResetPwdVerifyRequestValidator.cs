using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Authentication;

public class ResetPwdVerifyRequestValidator : AbstractValidator<ResetPasswordVerifyReqDto>
{
    public ResetPwdVerifyRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
    }
}