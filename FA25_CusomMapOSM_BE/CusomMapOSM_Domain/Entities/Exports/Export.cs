using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Exports;

public class Export
{
    public int ExportId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid MembershipId { get; set; }
    public required Guid MapId { get; set; }
    public required string FilePath { get; set; }
    public int FileSize { get; set; }
    public required Guid ExportTypeId { get; set; }
    public string QuotaType { get; set; } = "included";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = new();
    public Membership Membership { get; set; } = new();
    public Map Map { get; set; } = new();
    public ExportType ExportType { get; set; } = new();
}
