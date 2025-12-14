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


    public async Task<SupportTicketEntity> CreateSupportTicket(SupportTicketEntity supportTicket)
    {
        await _context.SupportTickets.AddAsync(supportTicket);
        await _context.SaveChangesAsync();
        return supportTicket;
    }

    public async Task<SupportTicketEntity> GetSupportTicketById(int ticketId)
    {
        var supportTicket = await _context.SupportTickets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    
        if (supportTicket == null)
        {
            throw new Exception("Support ticket not found");
        }
    
        return supportTicket;
    }

    public async Task<SupportTicketEntity> UpdateSupportTicket(SupportTicketEntity supportTicket)
    {
        _context.SupportTickets.Update(supportTicket);
        await _context.SaveChangesAsync();
        return supportTicket;
    }

    public async Task<bool> DeleteSupportTicket(int ticketId)
    {
        var supportTicket = await _context.SupportTickets.FindAsync(ticketId);
        if (supportTicket == null)
        {
            throw new Exception("Support ticket not found");
        }
        _context.SupportTickets.Remove(supportTicket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SupportTicketEntity>> GetSupportTickets(int page = 1, int pageSize = 20)
    {
        var supportTickets = await _context.SupportTickets
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return supportTickets;
    }

    public async Task<List<SupportTicketEntity>> GetSupportTicketsByUserId(Guid userId, int page = 1, int pageSize = 20)
    {
        var supportTickets = await _context.SupportTickets
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return supportTickets;
    }

    public async Task<int> GetSupportTicketsCount()
    {
        var supportTicketsCount = await _context.SupportTickets.CountAsync();
        return supportTicketsCount;
    }

    public async Task<int> GetSupportTicketsCountByUserId(Guid userId)
    {
        var supportTicketsCount = await _context.SupportTickets
            .Where(t => t.UserId == userId)
            .CountAsync();
        return supportTicketsCount;
    }

    public async Task<List<SupportTicketMessage>> GetSupportTicketMessages(int ticketId)
    {
        var supportTicketMessages = await _context.SupportTicketMessages
            .Where(m => m.TicketId == ticketId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
        return supportTicketMessages;
    }

    public async Task<SupportTicketMessage> GetSupportTicketMessageById(int messageId)
    {
        var supportTicketMessage = await _context.SupportTicketMessages.FindAsync(messageId);
        if (supportTicketMessage == null)
        {
            throw new Exception("Support ticket message not found");
        }
        return supportTicketMessage;
    }

    public async Task<SupportTicketMessage> CreateSupportTicketMessage(SupportTicketMessage supportTicketMessage)
    {
        await _context.SupportTicketMessages.AddAsync(supportTicketMessage);
        await _context.SaveChangesAsync();
        return supportTicketMessage;
    }

    public async Task<SupportTicketMessage> UpdateSupportTicketMessage(SupportTicketMessage supportTicketMessage)
    {
        _context.SupportTicketMessages.Update(supportTicketMessage);
        await _context.SaveChangesAsync();
        return supportTicketMessage;
    }

    public async Task<bool> DeleteSupportTicketMessage(int messageId)
    {
        var supportTicketMessage = await _context.SupportTicketMessages.FindAsync(messageId);
        if (supportTicketMessage == null)
        {
            throw new Exception("Support ticket message not found");
        }
        _context.SupportTicketMessages.Remove(supportTicketMessage);
        await _context.SaveChangesAsync();
        return true;
    }
}
