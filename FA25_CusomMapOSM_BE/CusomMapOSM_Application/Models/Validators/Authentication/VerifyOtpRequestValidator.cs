using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Authentication;

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpReqDto>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required");
    }
}