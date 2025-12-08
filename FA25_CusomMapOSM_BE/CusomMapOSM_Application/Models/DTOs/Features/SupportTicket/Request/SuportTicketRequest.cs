namespace CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Request;

public record CreateSupportTicketRequest
{
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public required string Priority { get; set; }
}

public record ResponseSupportTicketRequest
{
    public required string Response { get; set; }
}

public record ReplySupportTicketRequest
{
    public required string Reply { get; set; }
}