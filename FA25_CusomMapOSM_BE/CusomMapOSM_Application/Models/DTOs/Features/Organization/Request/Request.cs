namespace CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;

public record OrganizationReqDto
{
    public required string  OrgName { get; set; }
    public required string Abbreviation { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
}

public record InviteMemberOrganizationReqDto
{
    public required Guid  OrgId { get; set; }
    public required string  MemberEmail { get; set; }
    
    public required string MemberType { get; set; }
    
}

public record AcceptInviteOrganizationReqDto
{
    public required Guid InvitationId { get; set; }
}

public record GetOrganizationByIdReqDto
{
    public required Guid Id { get; set; }
}

public record UpdateOrganizationReqDto
{
    public required Guid Id { get; set; }
    public required string OrgName { get; set; }
    public required string Abbreviation { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
}

public record DeleteOrganizationReqDto
{
    public required Guid Id { get; set; }
}

public record UpdateMemberRoleReqDto
{
    public required Guid OrgId { get; set; }
    public required Guid MemberId { get; set; }
    public required string NewRole { get; set; }
}

public record RemoveMemberReqDto
{
    public required Guid OrgId { get; set; }
    public required Guid MemberId { get; set; }
}

public record RejectInviteOrganizationReqDto
{
    public required Guid InvitationId { get; set; }
}

public record CancelInviteOrganizationReqDto
{
    public required Guid InvitationId { get; set; }
}

public record TransferOwnershipReqDto
{
    public required Guid NewOwnerId { get; set; }
}
