using CusomMapOSM_Domain.Entities.Tickets;
using SupportTicketEntity = CusomMapOSM_Domain.Entities.Tickets.SupportTicket;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;

public interface ISupportTicketRepository
{
    Task<SupportTicketEntity> CreateSupportTicket(SupportTicketEntity supportTicket);
    Task<SupportTicketEntity> GetSupportTicketById(int ticketId);
    Task<SupportTicketEntity> UpdateSupportTicket(SupportTicketEntity supportTicket);
    Task<bool> DeleteSupportTicket(int ticketId);
    Task<List<SupportTicketEntity>> GetSupportTickets(int page = 1, int pageSize = 20);
    Task<int> GetSupportTicketsCount();
    Task<List<SupportTicketMessage>> GetSupportTicketMessages(int ticketId);
    Task<SupportTicketMessage> GetSupportTicketMessageById(int messageId);
    Task<SupportTicketMessage> CreateSupportTicketMessage(SupportTicketMessage supportTicketMessage);
    Task<SupportTicketMessage> UpdateSupportTicketMessage(SupportTicketMessage supportTicketMessage);
    Task<bool> DeleteSupportTicketMessage(int messageId);
}
