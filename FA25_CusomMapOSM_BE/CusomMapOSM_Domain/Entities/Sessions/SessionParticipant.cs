namespace CusomMapOSM_Domain.Entities.Sessions;

public class SessionParticipant
{
    public Guid SessionParticipantId { get; set; }
    public Guid SessionId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ParticipantKey { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsGuest { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int TotalScore { get; set; } = 0;
    public int TotalCorrect { get; set; } = 0;
    public int TotalAnswered { get; set; } = 0;
    public decimal AverageResponseTime { get; set; } = 0;
    public int Rank { get; set; } = 0;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Session? Session { get; set; }
}