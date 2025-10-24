namespace CusomMapOSM_Application.Models.DTOs.Features.Payment;

public record CancelPaymentResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
}
