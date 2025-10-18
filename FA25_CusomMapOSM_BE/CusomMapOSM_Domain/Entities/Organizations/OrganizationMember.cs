using System;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class OrganizationMember
{
    public Guid MemberId { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public OrganizationMemberTypeEnum Role { get; set; } = OrganizationMemberTypeEnum.Member;
    
    // Enhanced fields
    public Guid? InvitationId { get; set; }  // Link back to invitation (NEW)
    public Guid? InvitedBy { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;  // Replaced IsActive
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;  // Not nullable anymore
    public DateTime? LeftAt { get; set; }  // Track when member left (NEW)
    public string? LeaveReason { get; set; }  // Optional reason for leaving (NEW)

    // Navigation properties
    public Organization? Organization { get; set; }
    public User? User { get; set; }
    public User? Inviter { get; set; }
    public OrganizationInvitation? Invitation { get; set; }  // NEW - link to original invitation
}
