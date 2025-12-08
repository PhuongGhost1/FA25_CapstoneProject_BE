using CusomMapOSM_Application.Interfaces.Features.SupportTicket;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Request;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Response;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Tickets.Enums;
using Optional;
using SupportTicketEntity = CusomMapOSM_Domain.Entities.Tickets.SupportTicket;

namespace CusomMapOSM_Infrastructure.Features.SupportTicket;

public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ICurrentUserService _currentUserService;

    public SupportTicketService(ISupportTicketRepository supportTicketRepository,
        ICurrentUserService currentUserService)
    {
        _supportTicketRepository = supportTicketRepository;
        _currentUserService = currentUserService;
    }


    public async Task<Option<CreateSupportTicketResponse, Error>> CreateSupportTicket(
        CreateSupportTicketRequest request)
    {
        var userId = _currentUserService.GetUserId();
        var supportTicket = new SupportTicketEntity
        {
            UserId = userId.Value,
            Subject = request.Subject,
            Message = request.Message,
            Priority = request.Priority,
            Status = TicketStatusEnum.Open,
            CreatedAt = DateTime.UtcNow,
        };
        await _supportTicketRepository.CreateSupportTicket(supportTicket);
        return Option.Some<CreateSupportTicketResponse, Error>(new CreateSupportTicketResponse
        {
            TicketId = supportTicket.TicketId,
            Message = supportTicket.Message,
        });
    }

    public async Task<Option<SupportTicketListResponse, Error>> GetSupportTickets(int page = 1, int pageSize = 20)
    {
        var supportTickets = await _supportTicketRepository.GetSupportTickets(page, pageSize);
        var totalCount = await _supportTicketRepository.GetSupportTicketsCount();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Option.Some<SupportTicketListResponse, Error>(new SupportTicketListResponse
        {
            Tickets = supportTickets.Select(s => new SupportTicketDto
            {
                TicketId = s.TicketId,
                UserEmail = s.User.Email,
                UserName = s.User.FullName,
                Subject = s.Subject,
                Message = s.Message,
                Status = s.Status,
                Priority = s.Priority,
                CreatedAt = s.CreatedAt,
                ResolvedAt = s.ResolvedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }


    public async Task<Option<SupportTicketDto, Error>> GetSupportTicketById(int ticketId)
    {
        var supportTicket = await _supportTicketRepository.GetSupportTicketById(ticketId);
        if (supportTicket == null)
        {
            return Option.None<SupportTicketDto, Error>(Error.NotFound("SupportTicket.NotFound",
                "Support ticket not found"));
        }

        var supportTicketMessages = await _supportTicketRepository.GetSupportTicketMessages(ticketId);

        return Option.Some<SupportTicketDto, Error>(new SupportTicketDto
        {
            TicketId = supportTicket.TicketId,
            UserEmail = supportTicket.User.Email,
            UserName = supportTicket.User.FullName,
            Subject = supportTicket.Subject,
            Message = supportTicket.Message,
            Status = supportTicket.Status,
            Priority = supportTicket.Priority,
            CreatedAt = supportTicket.CreatedAt,
            ResolvedAt = supportTicket.ResolvedAt,
            Messages = supportTicketMessages.Select(m => new SupportTicketMessageDto
            {
                MessageId = m.MessageId,
                Message = m.Message,
                IsFromUser = m.IsFromUser,
                CreatedAt = m.CreatedAt,
            }).ToList(),
        });
    }

    public async Task<Option<ResponseSupportTicketResponse, Error>> ResponseToSupportTicket(int ticketId,
        ResponseSupportTicketRequest request)
    {
        var supportTicket = await _supportTicketRepository.GetSupportTicketById(ticketId);
        if (supportTicket == null)
        {
            return Option.None<ResponseSupportTicketResponse, Error>(Error.NotFound("SupportTicket.NotFound",
                "Support ticket not found"));
        }

        if (supportTicket.Status == TicketStatusEnum.Closed)
        {
            return Option.None<ResponseSupportTicketResponse, Error>(Error.Failure("SupportTicket.WasClose",
                "Support ticket is close"));
        }

        var supportTicketMessage = new SupportTicketMessage
        {
            TicketId = ticketId,
            Message = request.Response,
            IsFromUser = true,
            CreatedAt = DateTime.UtcNow,
        };
        await _supportTicketRepository.CreateSupportTicketMessage(supportTicketMessage);
        await _supportTicketRepository.UpdateSupportTicket(supportTicket);
        return Option.Some<ResponseSupportTicketResponse, Error>(new ResponseSupportTicketResponse
        {
            TicketId = supportTicket.TicketId,
            Message = request.Response,
        });
    }

    public async Task<Option<ReplySupportTicketResponse, Error>> ReplyToSupportTicket(int ticketId,
        ReplySupportTicketRequest request)
    {
        var supportTicket = await _supportTicketRepository.GetSupportTicketById(ticketId);
        if (supportTicket == null)
        {
            return Option.None<ReplySupportTicketResponse, Error>(Error.NotFound("SupportTicket.NotFound",
                "Support ticket not found"));
        }
        if (supportTicket.Status == TicketStatusEnum.Closed)
        {
            return Option.None<ReplySupportTicketResponse, Error>(Error.Failure("SupportTicket.WasClose",
                "Support ticket is close"));
        }

        var supportTicketMessage = new SupportTicketMessage
        {
            TicketId = ticketId,
            Message = request.Reply,
            IsFromUser = false,
            CreatedAt = DateTime.UtcNow,
        };
        await _supportTicketRepository.CreateSupportTicketMessage(supportTicketMessage);
        await _supportTicketRepository.UpdateSupportTicket(supportTicket);
        return Option.Some<ReplySupportTicketResponse, Error>(new ReplySupportTicketResponse
        {
            TicketId = supportTicket.TicketId,
            Message = request.Reply,
        });
    }

    public async Task<Option<bool, Error>> CloseSupportTicket(int ticketId)
    {
        var supportTicket = await _supportTicketRepository.GetSupportTicketById(ticketId);
        if (supportTicket == null)
        {
            return Option.None<bool, Error>(Error.NotFound("SupportTicket.NotFound", "Support ticket not found"));
        }

        supportTicket.Status = TicketStatusEnum.Closed;
        await _supportTicketRepository.UpdateSupportTicket(supportTicket);
        return Option.Some<bool, Error>(true);
    }
}