namespace CusomMapOSM_Domain.Entities;

public class MapEdit
{
    public Guid Id { get; set; }
    public string MapId { get; set; }
    public string UserId { get; set; }
    public string OperationType { get; set; }
    public string Data { get; set; }
    public DateTime Timestamp { get; set; }
    public int VersionNumber { get; set; }
    public bool IsReverted { get; set; }
}
