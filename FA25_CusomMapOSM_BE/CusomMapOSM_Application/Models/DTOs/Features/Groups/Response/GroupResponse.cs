namespace CusomMapOSM_Application.Models.DTOs.Features.Groups.Response;

public class GroupResponse
{
    public Guid GroupId { get; set; }
    public Guid SessionId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public List<GroupMemberResponse> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class GroupMemberResponse
{
    public Guid GroupMemberId { get; set; }
    public Guid SessionParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public bool IsLeader { get; set; }
    public DateTime JoinedAt { get; set; }
}
