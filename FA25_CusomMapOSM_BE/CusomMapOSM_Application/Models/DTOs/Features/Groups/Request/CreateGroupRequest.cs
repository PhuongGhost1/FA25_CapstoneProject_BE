namespace CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;

public class CreateGroupRequest
{
    public Guid SessionId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public List<Guid> MemberParticipantIds { get; set; } = new();
    public Guid? LeaderParticipantId { get; set; }
}
