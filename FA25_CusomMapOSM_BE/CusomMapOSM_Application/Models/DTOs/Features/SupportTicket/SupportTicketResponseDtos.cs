namespace CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;

public record CloseTicketResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
}
