using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapHistory
{
    public int VersionId { get; set; }
    public required Guid MapId { get; set; }
    public required Guid UserId { get; set; }
    public required string SnapshotData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
