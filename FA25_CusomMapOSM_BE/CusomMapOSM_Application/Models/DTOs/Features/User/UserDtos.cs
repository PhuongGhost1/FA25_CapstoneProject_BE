using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.User;

public record UserInfoDto
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public required string Role { get; set; }
    public required string AccountStatus { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public record GetUserInfoResponse
{
    public required UserInfoDto User { get; set; }
}

public record CurrentMembershipDto
{
    public required Guid MembershipId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid OrgId { get; set; }
    public required string OrgName { get; set; }
    public required int PlanId { get; set; }
    public required string PlanName { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required string Status { get; set; }
    public required bool AutoRenew { get; set; }
    public DateTime? LastResetDate { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record GetCurrentMembershipResponse
{
    public required CurrentMembershipDto Membership { get; set; }
}
