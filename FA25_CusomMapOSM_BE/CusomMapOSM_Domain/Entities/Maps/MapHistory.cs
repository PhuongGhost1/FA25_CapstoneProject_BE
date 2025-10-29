using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapHistory
{
    public Guid HistoryId { get; set; }
    public int HistoryVersion { get; set; }
    public required Guid MapId { get; set; }
    public required Guid UserId { get; set; }
    public required string SnapshotData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Map? Map { get; set; }
    public User? Creator { get; set; }
}
