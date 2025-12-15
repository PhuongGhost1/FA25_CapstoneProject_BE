using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Exports.Enums;
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
    public ExportStatusEnum Status { get; set; } = ExportStatusEnum.Pending;
    public string? ErrorMessage { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public User? User { get; set; }
    public Membership? Membership { get; set; }
    public Map? Map { get; set; }
    public ExportTypeEnum ExportType { get; set; }
}
