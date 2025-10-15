using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.SupportTicket;

public interface ISupportTicketService
{
    // Support Ticket CRUD operations for users
    Task<Option<SupportTicketListResponse, Error>> GetUserSupportTicketsAsync(Guid userId, int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default);
    Task<Option<SupportTicketDto, Error>> GetSupportTicketDetailsAsync(Guid userId, int ticketId, CancellationToken ct = default);
    Task<Option<CreateSupportTicketResponse, Error>> CreateSupportTicketAsync(Guid userId, CreateSupportTicketRequest request, CancellationToken ct = default);
    Task<Option<UpdateSupportTicketResponse, Error>> UpdateSupportTicketAsync(Guid userId, UpdateSupportTicketRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> CloseSupportTicketAsync(Guid userId, int ticketId, CancellationToken ct = default);

    // Support Ticket Messages
    Task<Option<AddTicketMessageResponse, Error>> AddTicketMessageAsync(Guid userId, AddTicketMessageRequest request, CancellationToken ct = default);
    Task<Option<List<SupportTicketMessageDto>, Error>> GetTicketMessagesAsync(Guid userId, int ticketId, CancellationToken ct = default);
}
