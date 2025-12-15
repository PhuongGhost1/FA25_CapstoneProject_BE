using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Workspaces.Enums;

namespace CusomMapOSM_Domain.Entities.Workspaces;

public class Workspace
{
    public Guid WorkspaceId { get; set; }
    public Guid? OrgId { get; set; }
    public Guid CreatedBy { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public WorkspaceAccessEnum Access { get; set; } = WorkspaceAccessEnum.AllMembers;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Organization? Organization { get; set; }
    public required User Creator { get; set; }
}
