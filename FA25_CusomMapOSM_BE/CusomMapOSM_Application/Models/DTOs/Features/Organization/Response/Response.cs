namespace CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;

public record OrganizationResDto
{
    public required string Result { get; set; }
}

public record InviteMemberOrganizationResDto
{
    public required string  Result { get; set; }
}

public record AcceptInviteOrganizationResDto
{
    public required string Result { get; set; }
}

public record InvitationDto
{
    public required Guid InvitationId { get; set; }
    public required Guid OrgId { get; set; }
    public required string OrgName { get; set; }
    public required string Email { get; set; }
    public required string InviterEmail { get; set; }
    public required string MemberType { get; set; }
    public required DateTime InvitedAt { get; set; }
    public required bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public record GetInvitationsResDto
{
    public required List<InvitationDto> Invitations { get; set; }
}

public record OrganizationDetailDto
{
    public required Guid OrgId { get; set; }
    public required string OrgName { get; set; }
    public required string Abbreviation { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required bool IsActive { get; set; }
}

public record GetAllOrganizationsResDto
{
    public required List<OrganizationDetailDto> Organizations { get; set; }
}

public record GetOrganizationByIdResDto
{
    public required OrganizationDetailDto Organization { get; set; }
}

public record UpdateOrganizationResDto
{
    public required string Result { get; set; }
}

public record DeleteOrganizationResDto
{
    public required string Result { get; set; }
}

public record MemberDto
{
    public required Guid MemberId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public required DateTime JoinedAt { get; set; }
    public required bool IsActive { get; set; }
}

public record GetOrganizationMembersResDto
{
    public required List<MemberDto> Members { get; set; }
}

public record UpdateMemberRoleResDto
{
    public required string Result { get; set; }
}

public record RemoveMemberResDto
{
    public required string Result { get; set; }
}

public record RejectInviteOrganizationResDto
{
    public required string Result { get; set; }
}

public record CancelInviteOrganizationResDto
{
    public required string Result { get; set; }
}

public record MyOrganizationDto
{
    public required Guid OrgId { get; set; }
    public required string OrgName { get; set; }
    public required string Abbreviation { get; set; }
    public required string MyRole { get; set; }
    public required DateTime JoinedAt { get; set; }
    public string? LogoUrl { get; set; }
}

public record GetMyOrganizationsResDto
{
    public required List<MyOrganizationDto> Organizations { get; set; }
}

public record TransferOwnershipResDto
{
    public required string Result { get; set; }
}
