using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Response;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.Templates.Email;
using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Shared.Constant;
using Optional;
using CusomMapOSM_Application.Common.ServiceConstants;
namespace CusomMapOSM_Infrastructure.Features.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly IJwtService _jwtService;
    private readonly IRedisCacheService _redisCacheService;
    private readonly HangfireEmailService _hangfireEmailService;

    public AuthenticationService(IAuthenticationRepository authenticationRepository, IJwtService jwtService,
        IRedisCacheService redisCacheService, ITypeRepository typeRepository, HangfireEmailService hangfireEmailService)
    {
        _authenticationRepository = authenticationRepository;
        _jwtService = jwtService;
        _redisCacheService = redisCacheService;
        _typeRepository = typeRepository;
        _hangfireEmailService = hangfireEmailService;
    }

    public async Task<Option<LoginResDto, Error>> Login(LoginReqDto req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
            return Option.None<LoginResDto, Error>(new Error("Authentication.InvalidEmailOrPassword", "Invalid email or password", ErrorType.Validation));

        var user = await _authenticationRepository.Login(req.Email, _jwtService.HashObject<string>(req.Password));
        if (user is null)
            return Option.None<LoginResDto, Error>(new Error("Authentication.InvalidEmailOrPassword", "Invalid email or password", ErrorType.Validation));

        var token = _jwtService.GenerateToken(user.UserId, user.Email, JwtConstant.ACCESS_TOKEN_EXP);
        if (string.IsNullOrEmpty(token))
            return Option.None<LoginResDto, Error>(new Error("Authentication.InvalidToken", "Invalid token", ErrorType.Validation));

        return Option.Some<LoginResDto, Error>(new LoginResDto { Token = token });
    }

    public async Task<Option<RegisterResDto, Error>> LogOut(Guid userId)
    {
        var user = await _authenticationRepository.GetUserById(userId);
        if (user is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound));

        var accountStatus = await _typeRepository.GetAccountStatusById(AccountStatusEnum.Inactive);

        user.AccountStatusId = accountStatus.StatusId;

        await _authenticationRepository.UpdateUser(user);

        await _redisCacheService.ForceLogout(userId);

        return Option.Some<RegisterResDto, Error>(new RegisterResDto { Result = "Logout successfully" });
    }

    public async Task<Option<RegisterResDto, Error>> VerifyEmail(RegisterVerifyReqDto req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.FirstName) || string.IsNullOrEmpty(req.LastName))
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidEmailOrPassword", "Invalid email or password", ErrorType.Validation));

        var isEmailExists = await _authenticationRepository.IsEmailExists(req.Email);
        if (isEmailExists)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.EmailAlreadyExists", "Email already exists", ErrorType.Validation));

        var userRole = await _typeRepository.GetUserRoleById(UserRoleEnum.RegisteredUser);
        var accountStatus = await _typeRepository.GetAccountStatusById(AccountStatusEnum.PendingVerification);

        var user = new DomainUser.User
        {
            Email = req.Email,
            PasswordHash = _jwtService.HashObject<string>(req.Password),
            FullName = $"{req.FirstName} {req.LastName}",
            Phone = req.Phone,
            RoleId = userRole.RoleId,
            AccountStatusId = accountStatus.StatusId,
            CreatedAt = DateTime.UtcNow,
        };

        await _authenticationRepository.Register(user);

        var otp = new Random().Next(100000, 999999).ToString();

        await _redisCacheService.Set(otp, new RegisterVerifyOtpResDto { Email = req.Email, Otp = otp }, TimeSpan.FromMinutes(10));

        var mail = new MailRequest
        {
            ToEmail = req.Email,
            Subject = "Verify your email",
            Body = EmailTemplates.Authentication.GetEmailVerificationOtpTemplate(otp)
        };

        _hangfireEmailService.EnqueueEmail(mail);

        return Option.Some<RegisterResDto, Error>(new RegisterResDto { Result = "Email sent successfully" });
    }

    public async Task<Option<RegisterResDto, Error>> VerifyOtp(VerifyOtpReqDto req)
    {
        if (string.IsNullOrEmpty(req.Otp))
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation));

        var otp = await _redisCacheService.Get<RegisterVerifyOtpResDto>(req.Otp);
        if (otp is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation));

        if (otp.Otp != req.Otp)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation));

        var user = await _authenticationRepository.GetUserByEmail(otp.Email);
        if (user is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound));

        var accountStatus = await _typeRepository.GetAccountStatusById(AccountStatusEnum.Active);

        user.AccountStatusId = accountStatus.StatusId;
        await _authenticationRepository.UpdateUser(user);

        // Grant default Free membership access tools (IDs 1-11)
        var freeAccessToolIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var expiryDate = DateTime.UtcNow.AddYears(1); // Free access tools expire in 1 year

        var grantResult = await _userAccessToolService.GrantAccessToToolsAsync(user.UserId, freeAccessToolIds, expiryDate, CancellationToken.None);
        if (!grantResult.HasValue)
        {
            // Log the error but don't fail the registration process
            var error = grantResult.Match(
                some: _ => (Error)null!,
                none: err => err
            );
            Console.WriteLine($"Failed to grant default access tools for user {user.UserId}: {error?.Description}");
        }

        await _redisCacheService.Remove(req.Otp);

        return Option.Some<RegisterResDto, Error>(new RegisterResDto { Result = "Email verified successfully" });
    }

    public async Task<Option<RegisterResDto, Error>> ResetPasswordVerify(ResetPasswordVerifyReqDto req)
    {
        if (string.IsNullOrEmpty(req.Email))
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidEmail", "Invalid email", ErrorType.Validation));

        var user = await _authenticationRepository.GetUserByEmail(req.Email);
        if (user is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound));

        var otp = new Random().Next(100000, 999999).ToString();

        await _redisCacheService.Set(otp, new RegisterVerifyOtpResDto { Email = req.Email, Otp = otp }, TimeSpan.FromMinutes(10));

        var mail = new MailRequest
        {
            ToEmail = req.Email,
            Subject = "Reset your password",
            Body = EmailTemplates.Authentication.GetPasswordResetOtpTemplate(otp)
        };

        _hangfireEmailService.EnqueueEmail(mail);

        return Option.Some<RegisterResDto, Error>(new RegisterResDto { Result = "OTP sent successfully" });
    }

    public async Task<Option<RegisterResDto, Error>> ResetPassword(ResetPasswordReqDto req)
    {
        if (string.IsNullOrEmpty(req.NewPassword) || string.IsNullOrEmpty(req.ConfirmPassword))
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidPassword", "Invalid password", ErrorType.Validation));

        if (req.NewPassword != req.ConfirmPassword)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.PasswordMismatch", "Password mismatch", ErrorType.Validation));

        var otp = await _redisCacheService.Get<RegisterVerifyOtpResDto>(req.Otp);
        if (otp is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation));

        if (otp.Otp != req.Otp)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation));

        var user = await _authenticationRepository.GetUserByEmail(otp.Email);
        if (user is null)
            return Option.None<RegisterResDto, Error>(new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound));

        user.PasswordHash = _jwtService.HashObject<string>(req.NewPassword);
        await _authenticationRepository.UpdateUser(user);

        await _redisCacheService.Remove(otp.Otp);

        return Option.Some<RegisterResDto, Error>(new RegisterResDto { Result = "Password reset successfully" });
    }
}