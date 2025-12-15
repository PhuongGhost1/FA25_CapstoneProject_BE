using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Request;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.SupportTicket;

public interface ISupportTicketService
{
    Task<Option<CreateSupportTicketResponse, Error>> CreateSupportTicket(CreateSupportTicketRequest request);
    Task<Option<SupportTicketListResponse, Error>> GetSupportTickets(int page = 1, int pageSize = 20);
    Task<Option<SupportTicketDto, Error>> GetSupportTicketById(int ticketId);
    Task<Option<ResponseSupportTicketResponse, Error>> ResponseToSupportTicket(int ticketId, ResponseSupportTicketRequest request);
    Task<Option<ReplySupportTicketResponse, Error>> ReplyToSupportTicket(int ticketId, ReplySupportTicketRequest request);
    Task<Option<bool, Error>> CloseSupportTicket(int ticketId);
}
