namespace CusomMapOSM_Application.Interfaces;

public record MapEditOperation
{
    public string Type { get; init; } = default!;
    public object Data { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public DateTime Timestamp { get; init; }
    public bool IsReverted { get; set; }
}
