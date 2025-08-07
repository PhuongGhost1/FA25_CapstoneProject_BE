namespace CusomMapOSM_Application.Models.DTOs.Services;

public record VerifyOtpRequest
{
    public required string Otp { get; set; }
    public required string Email { get; set; }
}