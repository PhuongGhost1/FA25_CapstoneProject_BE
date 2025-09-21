using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Tickets.Enums;

namespace CusomMapOSM_Domain.Entities.Tickets;

public class SupportTicket
{
    public int TicketId { get; set; }
    public required Guid UserId { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public TicketStatusEnum Status { get; set; } = TicketStatusEnum.Open;
    public string Priority { get; set; } = "low";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public User User { get; set; } = new();
}
