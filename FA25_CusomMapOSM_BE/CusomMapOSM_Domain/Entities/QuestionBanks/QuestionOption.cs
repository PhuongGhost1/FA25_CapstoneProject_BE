namespace CusomMapOSM_Domain.Entities.QuestionBanks;

public class QuestionOption
{
    public Guid QuestionOptionId { get; set; }
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
    public bool IsCorrect { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Question? Question { get; set; }
}