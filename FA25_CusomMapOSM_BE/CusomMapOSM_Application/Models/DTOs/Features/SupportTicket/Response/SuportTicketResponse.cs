using CusomMapOSM_Domain.Entities.Tickets.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Response;

public record CreateSupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
}

public record SupportTicketListResponse
{
    public required List<SupportTicketDto> Tickets { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record SupportTicketDto
{
    public required int TicketId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public required TicketStatusEnum Status { get; set; }
    public required string Priority { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<SupportTicketMessageDto> Messages { get; set; } = new List<SupportTicketMessageDto>();
}
public record SupportTicketMessageDto
{
    public required int MessageId { get; set; }
    public string? Message { get; set; }
    public bool? IsFromUser { get; set; }
    public required DateTime CreatedAt { get; set; }
}

public record ResponseSupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
}

public record ReplySupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
}

public record CloseSupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
}