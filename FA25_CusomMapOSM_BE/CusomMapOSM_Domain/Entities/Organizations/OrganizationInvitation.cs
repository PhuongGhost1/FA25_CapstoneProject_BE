using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class OrganizationInvitation
{
    public Guid InvitationId { get; set; }
    public Guid OrgId { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid InvitedBy { get; set; }
    public OrganizationMemberTypeEnum Role { get; set; } = OrganizationMemberTypeEnum.Member;
    
    // Enhanced fields
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string? InvitationToken { get; set; }  // For verification link
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);  // 7 days expiry
    public DateTime? RespondedAt { get; set; }  // Renamed from AcceptedAt for clarity
    public string? Message { get; set; }  // Optional invitation message

    // Navigation properties
    public Organization? Organization { get; set; }
    public User? Inviter { get; set; }
}
