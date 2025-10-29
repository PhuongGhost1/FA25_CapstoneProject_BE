namespace CusomMapOSM_Application.Models.StoryMaps;

public class StoryBroadcastState
{
    public Guid MapId { get; set; }
    public Guid? StoryMapId { get; set; }
    public int? SegmentIndex { get; set; }
    public int? StepIndex { get; set; }
    public double[]? ViewCenter { get; set; }
    public double? Zoom { get; set; }
    public IDictionary<string, object>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class StoryBroadcastSessionInfo
{
    public string SessionCode { get; set; } = string.Empty;
    public Guid MapId { get; set; }
    public Guid? StoryMapId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public StoryBroadcastState? LastState { get; set; }
}
