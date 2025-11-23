using CusomMapOSM_Domain.Entities.Sessions;

namespace CusomMapOSM_Domain.Entities.Groups;

public class SessionGroupMember
{
    public Guid GroupMemberId { get; set; }
    public Guid GroupId { get; set; }
    public Guid SessionParticipantId { get; set; }
    public bool IsLeader { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public SessionGroup? Group { get; set; }
    public SessionParticipant? SessionParticipant { get; set; }
}
