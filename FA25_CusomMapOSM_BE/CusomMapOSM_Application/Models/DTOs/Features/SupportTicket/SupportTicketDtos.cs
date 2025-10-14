using CusomMapOSM_Domain.Entities.Tickets.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;

// Request DTOs
public record CreateSupportTicketRequest
{
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public string Priority { get; set; } = "low";
}

public record UpdateSupportTicketRequest
{
    public required int TicketId { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? Priority { get; set; }
}

public record AddTicketMessageRequest
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
}

// Response DTOs
public record SupportTicketDto
{
    public required int TicketId { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public required TicketStatusEnum Status { get; set; }
    public required string Priority { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<SupportTicketMessageDto> Messages { get; set; } = new();
}

public record SupportTicketMessageDto
{
    public required int MessageId { get; set; }
    public required int TicketId { get; set; }
    public required string Message { get; set; }
    public required bool IsFromUser { get; set; }
    public required DateTime CreatedAt { get; set; }
}

public record SupportTicketListResponse
{
    public required IReadOnlyList<SupportTicketDto> Tickets { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record CreateSupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record UpdateSupportTicketResponse
{
    public required int TicketId { get; set; }
    public required string Message { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record AddTicketMessageResponse
{
    public required int MessageId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
