using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class OrganizationInvitation
{
    public Guid InvitationId { get; set; }
    public Guid OrgId { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid InvitedBy { get; set; }
    public Guid MembersRoleId { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }

    public Organization? Organization { get; set; }
    public OrganizationMemberType? Role { get; set; }
    public User? Inviter { get; set; }
}
