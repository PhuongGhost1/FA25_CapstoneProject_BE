using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class CreateMapLegendItemRequest
{
    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(10)]
    public string Emoji { get; set; } = "📍";
    
    [MaxLength(500)]
    [Url]
    public string? IconUrl { get; set; }
    
    [MaxLength(20)]
    public string? Color { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsVisible { get; set; } = true;
}

public class UpdateMapLegendItemRequest
{
    [MaxLength(100)]
    public string? Label { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(10)]
    public string? Emoji { get; set; }
    
    [MaxLength(500)]
    [Url]
    public string? IconUrl { get; set; }
    
    [MaxLength(20)]
    public string? Color { get; set; }
    
    public int? DisplayOrder { get; set; }
    
    public bool? IsVisible { get; set; }
}

public class ReorderMapLegendItemsRequest
{
    [Required]
    public List<Guid> ItemIds { get; set; } = [];
}
