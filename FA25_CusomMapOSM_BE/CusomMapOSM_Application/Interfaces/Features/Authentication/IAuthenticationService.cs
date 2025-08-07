using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Authentication;
public interface IAuthenticationService
{
    Task<Option<LoginResDto, Error>> Login(LoginReqDto req);
    Task<Option<RegisterResDto, Error>> LogOut(Guid userId);
    Task<Option<RegisterResDto, Error>> VerifyEmail(RegisterVerifyReqDto req);
    Task<Option<RegisterResDto, Error>> VerifyOtp(VerifyOtpReqDto req);
    Task<Option<RegisterResDto, Error>> ResetPasswordVerify(ResetPasswordVerifyReqDto req);
    Task<Option<RegisterResDto, Error>> ResetPassword(ResetPasswordReqDto req);
}