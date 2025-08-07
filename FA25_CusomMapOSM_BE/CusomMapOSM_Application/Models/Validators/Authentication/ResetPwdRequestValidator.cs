using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Authentication;

public class ResetPwdRequestValidator : AbstractValidator<ResetPasswordReqDto>
{
    public ResetPwdRequestValidator()
    {
        RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm password is required");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Confirm password must match new password");
    }
}