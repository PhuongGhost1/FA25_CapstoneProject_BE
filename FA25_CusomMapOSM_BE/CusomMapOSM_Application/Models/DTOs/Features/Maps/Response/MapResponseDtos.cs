namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public record LayerDataResponse
{
    public required object LayerData { get; set; }
}

public record MapSnapshotResponse
{
    public required string Snapshot { get; set; }
}

public record ApplySnapshotResponse
{
    public required bool Applied { get; set; }
}

public record DeleteFeatureResponse
{
    public required bool Deleted { get; set; }
}

public record ImportStoryResponse
{
    public required bool Imported { get; set; }
}

public record CanEditResponse
{
    public required bool CanEdit { get; set; }
}
