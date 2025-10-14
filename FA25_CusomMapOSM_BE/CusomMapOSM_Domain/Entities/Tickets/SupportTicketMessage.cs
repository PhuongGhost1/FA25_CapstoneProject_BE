using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Tickets;

public class SupportTicketMessage
{
    public int MessageId { get; set; }
    public required int TicketId { get; set; }
    public string? Message { get; set; }
    public bool IsFromUser { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SupportTicket SupportTicket { get; set; } = null!; // Nullable
}
