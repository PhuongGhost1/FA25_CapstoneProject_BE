namespace CusomMapOSM_Application.Models.DTOs.Features.Faqs;

public record FaqDto
{
    public required int FaqId { get; set; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public required string Category { get; set; }
    public required DateTime CreatedAt { get; set; }
}

public record GetAllFaqsResponse
{
    public required List<FaqDto> Faqs { get; set; }
}

public record GetFaqsByCategoryResponse
{
    public required string Category { get; set; }
    public required List<FaqDto> Faqs { get; set; }
}

public record GetFaqByIdResponse
{
    public required FaqDto Faq { get; set; }
}

public record GetFaqCategoriesResponse
{
    public required List<string> Categories { get; set; }
}
