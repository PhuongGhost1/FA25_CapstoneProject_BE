namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;

public class QuestionDTO
{
    public Guid QuestionId { get; set; }
    public Guid QuestionBankId { get; set; }
    public Guid? LocationId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public string? QuestionAudioUrl { get; set; }
    public int Points { get; set; }
    public int TimeLimit { get; set; }
    public string? CorrectAnswerText { get; set; }
    public decimal? CorrectLatitude { get; set; }
    public decimal? CorrectLongitude { get; set; }
    public int? AcceptanceRadiusMeters { get; set; }
    public string? HintText { get; set; }
    public string? Explanation { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<QuestionOptionDTO>? Options { get; set; }
}

public class QuestionOptionDTO
{
    public Guid QuestionOptionId { get; set; }
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}

