namespace CusomMapOSM_Application.Models.DTOs.Services.UserAsset.Response;

public record UserAssetListResponse
{
    public required IReadOnlyList<UserAssetResponse> Assets { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalPages { get; set; }
}

public record UserAssetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Type { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}