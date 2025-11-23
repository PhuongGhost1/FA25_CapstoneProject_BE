using CusomMapOSM_Domain.Entities.Sessions;

namespace CusomMapOSM_Domain.Entities.Groups;

public class SessionGroup
{
    public Guid GroupId { get; set; }
    public Guid SessionId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Session? Session { get; set; }
}
