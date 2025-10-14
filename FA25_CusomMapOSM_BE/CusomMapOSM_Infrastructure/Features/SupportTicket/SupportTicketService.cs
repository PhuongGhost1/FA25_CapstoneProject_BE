using CusomMapOSM_Application.Interfaces.Features.SupportTicket;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Tickets.Enums;
using Optional;
using Optional.Unsafe;
using SupportTicketEntity = CusomMapOSM_Domain.Entities.Tickets.SupportTicket;

namespace CusomMapOSM_Infrastructure.Features.SupportTicket;

public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _supportTicketRepository;

    public SupportTicketService(ISupportTicketRepository supportTicketRepository)
    {
        _supportTicketRepository = supportTicketRepository;
    }

    public async Task<Option<SupportTicketListResponse, Error>> GetUserSupportTicketsAsync(Guid userId, int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default)
    {
        try
        {
            var tickets = await _supportTicketRepository.GetUserSupportTicketsAsync(userId, page, pageSize, status, ct);
            var totalCount = await _supportTicketRepository.GetUserSupportTicketsCountAsync(userId, status, ct);

            var ticketDtos = tickets.Select(t => new SupportTicketDto
            {
                TicketId = t.TicketId,
                Subject = t.Subject ?? "No Subject",
                Message = t.Message ?? "",
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                ResolvedAt = t.ResolvedAt,
                Messages = new List<SupportTicketMessageDto>() // Would need to load messages separately
            }).ToList();

            return Option.Some<SupportTicketListResponse, Error>(new SupportTicketListResponse
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return Option.None<SupportTicketListResponse, Error>(Error.Failure("SupportTicket.GetTicketsFailed", $"Failed to get support tickets: {ex.Message}"));
        }
    }

    public async Task<Option<SupportTicketDto, Error>> GetSupportTicketDetailsAsync(Guid userId, int ticketId, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _supportTicketRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<SupportTicketDto, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            // Verify user owns this ticket
            if (ticket.UserId != userId)
            {
                return Option.None<SupportTicketDto, Error>(Error.Forbidden("Ticket.AccessDenied", "You don't have access to this support ticket"));
            }

            var messages = await _supportTicketRepository.GetTicketMessagesAsync(ticketId, ct);
            var messageDtos = messages.Select(m => new SupportTicketMessageDto
            {
                MessageId = m.MessageId,
                TicketId = m.TicketId,
                Message = m.Message ?? "",
                IsFromUser = m.IsFromUser,
                CreatedAt = m.CreatedAt
            }).ToList();

            var ticketDto = new SupportTicketDto
            {
                TicketId = ticket.TicketId,
                Subject = ticket.Subject ?? "No Subject",
                Message = ticket.Message ?? "",
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                Messages = messageDtos
            };

            return Option.Some<SupportTicketDto, Error>(ticketDto);
        }
        catch (Exception ex)
        {
            return Option.None<SupportTicketDto, Error>(Error.Failure("SupportTicket.GetTicketDetailsFailed", $"Failed to get support ticket details: {ex.Message}"));
        }
    }

    public async Task<Option<CreateSupportTicketResponse, Error>> CreateSupportTicketAsync(Guid userId, CreateSupportTicketRequest request, CancellationToken ct = default)
    {
        try
        {
            var ticket = new SupportTicketEntity
            {
                UserId = userId,
                Subject = request.Subject,
                Message = request.Message,
                Status = TicketStatusEnum.Open,
                Priority = request.Priority,
                CreatedAt = DateTime.UtcNow
            };

            var success = await _supportTicketRepository.CreateSupportTicketAsync(ticket, ct);
            if (!success)
            {
                return Option.None<CreateSupportTicketResponse, Error>(Error.Failure("SupportTicket.CreateFailed", "Failed to create support ticket"));
            }

            return Option.Some<CreateSupportTicketResponse, Error>(new CreateSupportTicketResponse
            {
                TicketId = ticket.TicketId,
                Message = "Support ticket created successfully",
                CreatedAt = ticket.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Option.None<CreateSupportTicketResponse, Error>(Error.Failure("SupportTicket.CreateFailed", $"Failed to create support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<UpdateSupportTicketResponse, Error>> UpdateSupportTicketAsync(Guid userId, UpdateSupportTicketRequest request, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _supportTicketRepository.GetSupportTicketByIdAsync(request.TicketId, ct);
            if (ticket == null)
            {
                return Option.None<UpdateSupportTicketResponse, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            // Verify user owns this ticket
            if (ticket.UserId != userId)
            {
                return Option.None<UpdateSupportTicketResponse, Error>(Error.Forbidden("Ticket.AccessDenied", "You don't have access to this support ticket"));
            }

            // Only allow updates if ticket is not closed
            if (ticket.Status == TicketStatusEnum.Closed)
            {
                return Option.None<UpdateSupportTicketResponse, Error>(Error.ValidationError("Ticket.Closed", "Cannot update a closed support ticket"));
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Subject)) ticket.Subject = request.Subject;
            if (!string.IsNullOrEmpty(request.Message)) ticket.Message = request.Message;
            if (!string.IsNullOrEmpty(request.Priority)) ticket.Priority = request.Priority;

            var success = await _supportTicketRepository.UpdateSupportTicketAsync(ticket, ct);
            if (!success)
            {
                return Option.None<UpdateSupportTicketResponse, Error>(Error.Failure("SupportTicket.UpdateFailed", "Failed to update support ticket"));
            }

            return Option.Some<UpdateSupportTicketResponse, Error>(new UpdateSupportTicketResponse
            {
                TicketId = request.TicketId,
                Message = "Support ticket updated successfully",
                UpdatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Option.None<UpdateSupportTicketResponse, Error>(Error.Failure("SupportTicket.UpdateFailed", $"Failed to update support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CloseSupportTicketAsync(Guid userId, int ticketId, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _supportTicketRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            // Verify user owns this ticket
            if (ticket.UserId != userId)
            {
                return Option.None<bool, Error>(Error.Forbidden("Ticket.AccessDenied", "You don't have access to this support ticket"));
            }

            // Only allow closing if ticket is not already closed
            if (ticket.Status == TicketStatusEnum.Closed)
            {
                return Option.None<bool, Error>(Error.ValidationError("Ticket.AlreadyClosed", "Support ticket is already closed"));
            }

            ticket.Status = TicketStatusEnum.Closed;
            ticket.ResolvedAt = DateTime.UtcNow;

            var success = await _supportTicketRepository.UpdateSupportTicketAsync(ticket, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SupportTicket.CloseFailed", $"Failed to close support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<AddTicketMessageResponse, Error>> AddTicketMessageAsync(Guid userId, AddTicketMessageRequest request, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _supportTicketRepository.GetSupportTicketByIdAsync(request.TicketId, ct);
            if (ticket == null)
            {
                return Option.None<AddTicketMessageResponse, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            // Verify user owns this ticket
            if (ticket.UserId != userId)
            {
                return Option.None<AddTicketMessageResponse, Error>(Error.Forbidden("Ticket.AccessDenied", "You don't have access to this support ticket"));
            }

            // Only allow adding messages if ticket is not closed
            if (ticket.Status == TicketStatusEnum.Closed)
            {
                return Option.None<AddTicketMessageResponse, Error>(Error.ValidationError("Ticket.Closed", "Cannot add messages to a closed support ticket"));
            }

            var message = new SupportTicketMessage
            {
                TicketId = request.TicketId,
                Message = request.Message,
                IsFromUser = true,
                CreatedAt = DateTime.UtcNow
            };

            var success = await _supportTicketRepository.AddTicketMessageAsync(message, ct);
            if (!success)
            {
                return Option.None<AddTicketMessageResponse, Error>(Error.Failure("SupportTicket.AddMessageFailed", "Failed to add message to support ticket"));
            }

            return Option.Some<AddTicketMessageResponse, Error>(new AddTicketMessageResponse
            {
                MessageId = message.MessageId,
                Message = "Message added successfully",
                CreatedAt = message.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Option.None<AddTicketMessageResponse, Error>(Error.Failure("SupportTicket.AddMessageFailed", $"Failed to add message to support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<List<SupportTicketMessageDto>, Error>> GetTicketMessagesAsync(Guid userId, int ticketId, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _supportTicketRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<List<SupportTicketMessageDto>, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            // Verify user owns this ticket
            if (ticket.UserId != userId)
            {
                return Option.None<List<SupportTicketMessageDto>, Error>(Error.Forbidden("Ticket.AccessDenied", "You don't have access to this support ticket"));
            }

            var messages = await _supportTicketRepository.GetTicketMessagesAsync(ticketId, ct);
            var messageDtos = messages.Select(m => new SupportTicketMessageDto
            {
                MessageId = m.MessageId,
                TicketId = m.TicketId,
                Message = m.Message ?? "",
                IsFromUser = m.IsFromUser,
                CreatedAt = m.CreatedAt
            }).ToList();

            return Option.Some<List<SupportTicketMessageDto>, Error>(messageDtos);
        }
        catch (Exception ex)
        {
            return Option.None<List<SupportTicketMessageDto>, Error>(Error.Failure("SupportTicket.GetMessagesFailed", $"Failed to get ticket messages: {ex.Message}"));
        }
    }
}
