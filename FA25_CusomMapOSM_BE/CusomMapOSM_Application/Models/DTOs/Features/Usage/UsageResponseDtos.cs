namespace CusomMapOSM_Application.Models.DTOs.Features.Usage;

public record ConsumeQuotaResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
}
