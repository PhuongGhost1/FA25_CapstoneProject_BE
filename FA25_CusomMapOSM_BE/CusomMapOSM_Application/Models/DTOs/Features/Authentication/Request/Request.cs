namespace CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;

public class LoginReqDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterVerifyReqDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Phone { get; set; }
}

public class VerifyOtpReqDto
{
    public required string Otp { get; set; }
}

public class ResetPasswordVerifyReqDto
{
    public required string Email { get; set; }
}

public class ResetPasswordReqDto
{
    public required string Otp { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}