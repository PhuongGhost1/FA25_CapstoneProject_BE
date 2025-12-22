namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapLegendItemDto
{
    public Guid LegendItemId { get; set; }
    public Guid MapId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Emoji { get; set; } = "📍";
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetMapLegendItemsResponse
{
    public IReadOnlyCollection<MapLegendItemDto> Items { get; set; } = [];
}

public class CreateMapLegendItemResponse
{
    public Guid LegendItemId { get; set; }
    public string Message { get; set; } = "Legend item created successfully";
}

public class UpdateMapLegendItemResponse
{
    public string Message { get; set; } = "Legend item updated successfully";
}

public class DeleteMapLegendItemResponse
{
    public string Message { get; set; } = "Legend item deleted successfully";
}

public class ReorderMapLegendItemsResponse
{
    public string Message { get; set; } = "Legend items reordered successfully";
}
