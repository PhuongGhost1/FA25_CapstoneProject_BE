using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Notifications;

public class Notification
{
    public int NotificationId { get; set; }
    public required Guid UserId { get; set; }
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }

    public User User { get; set; } = new();
}
