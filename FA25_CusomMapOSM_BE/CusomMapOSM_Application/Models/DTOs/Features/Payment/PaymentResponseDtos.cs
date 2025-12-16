namespace CusomMapOSM_Application.Models.DTOs.Features.Payment;

public record CancelPaymentResponse
{
    public required bool Success { get; set; }
    public required string TransactionId { get; set; }
    public required string NewStatus { get; set; }
    public required string CancellationReason { get; set; }
    public required string Message { get; set; }
}
