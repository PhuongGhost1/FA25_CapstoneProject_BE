namespace CusomMapOSM_Application.Models.DTOs.Services.MinIO;

public record MinIOFileMetadata
{
    public string ObjectName { get; init; } = string.Empty;
    public long Size { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
    public string ETag { get; init; } = string.Empty;
}
