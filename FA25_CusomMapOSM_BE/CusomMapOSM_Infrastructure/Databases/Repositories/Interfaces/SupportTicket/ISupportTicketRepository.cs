using CusomMapOSM_Domain.Entities.Tickets;
using SupportTicketEntity = CusomMapOSM_Domain.Entities.Tickets.SupportTicket;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;

public interface ISupportTicketRepository
{
    // Support Ticket operations
    Task<List<SupportTicketEntity>> GetUserSupportTicketsAsync(Guid userId, int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default);
    Task<int> GetUserSupportTicketsCountAsync(Guid userId, string? status = null, CancellationToken ct = default);
    Task<SupportTicketEntity?> GetSupportTicketByIdAsync(int ticketId, CancellationToken ct = default);
    Task<bool> CreateSupportTicketAsync(SupportTicketEntity ticket, CancellationToken ct = default);
    Task<bool> UpdateSupportTicketAsync(SupportTicketEntity ticket, CancellationToken ct = default);
    Task<bool> DeleteSupportTicketAsync(int ticketId, CancellationToken ct = default);

    // Support Ticket Message operations
    Task<List<SupportTicketMessage>> GetTicketMessagesAsync(int ticketId, CancellationToken ct = default);
    Task<bool> AddTicketMessageAsync(SupportTicketMessage message, CancellationToken ct = default);
    Task<bool> UpdateTicketMessageAsync(SupportTicketMessage message, CancellationToken ct = default);
    Task<bool> DeleteTicketMessageAsync(int messageId, CancellationToken ct = default);
}
