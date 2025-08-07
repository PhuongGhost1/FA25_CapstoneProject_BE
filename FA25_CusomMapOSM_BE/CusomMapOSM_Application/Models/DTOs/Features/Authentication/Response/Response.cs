namespace CusomMapOSM_Application.Models.DTOs.Features.Authentication.Response;

public record LoginResDto
{
    public required string Token { get; set; }
}

public record RegisterResDto
{
    public required string Result { get; set; }
}

public record RegisterVerifyOtpResDto
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}