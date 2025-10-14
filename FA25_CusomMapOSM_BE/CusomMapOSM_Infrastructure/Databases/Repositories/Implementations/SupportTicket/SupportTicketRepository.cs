using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Tickets.Enums;
using Microsoft.EntityFrameworkCore;
using SupportTicketEntity = CusomMapOSM_Domain.Entities.Tickets.SupportTicket;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.SupportTicket;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SupportTicketRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupportTicketEntity>> GetUserSupportTicketsAsync(Guid userId, int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default)
    {
        var query = _context.SupportTickets
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatusEnum>(status, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetUserSupportTicketsCountAsync(Guid userId, string? status = null, CancellationToken ct = default)
    {
        var query = _context.SupportTickets
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatusEnum>(status, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        return await query.CountAsync(ct);
    }

    public async Task<SupportTicketEntity?> GetSupportTicketByIdAsync(int ticketId, CancellationToken ct = default)
    {
        return await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TicketId == ticketId, ct);
    }

    public async Task<bool> CreateSupportTicketAsync(SupportTicketEntity ticket, CancellationToken ct = default)
    {
        try
        {
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateSupportTicketAsync(SupportTicketEntity ticket, CancellationToken ct = default)
    {
        try
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteSupportTicketAsync(int ticketId, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId, ct);
            if (ticket == null) return false;

            _context.SupportTickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SupportTicketMessage>> GetTicketMessagesAsync(int ticketId, CancellationToken ct = default)
    {
        return await _context.SupportTicketMessages
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> AddTicketMessageAsync(SupportTicketMessage message, CancellationToken ct = default)
    {
        try
        {
            _context.SupportTicketMessages.Add(message);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateTicketMessageAsync(SupportTicketMessage message, CancellationToken ct = default)
    {
        try
        {
            _context.SupportTicketMessages.Update(message);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteTicketMessageAsync(int messageId, CancellationToken ct = default)
    {
        try
        {
            var message = await _context.SupportTicketMessages.FirstOrDefaultAsync(m => m.MessageId == messageId, ct);
            if (message == null) return false;

            _context.SupportTicketMessages.Remove(message);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
