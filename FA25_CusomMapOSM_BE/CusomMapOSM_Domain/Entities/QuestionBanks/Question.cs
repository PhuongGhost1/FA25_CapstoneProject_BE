using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Domain.Entities.QuestionBanks;

public class Question
{
    public Guid QuestionId { get; set; }
    public Guid QuestionBankId { get; set; }
    public Guid? LocationId { get; set; }
    public QuestionTypeEnum QuestionType { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public string? QuestionAudioUrl { get; set; }
    public int Points { get; set; } = 100;
    public int TimeLimit { get; set; } = 30;
    public string? CorrectAnswerText { get; set; }
    public decimal? CorrectLatitude { get; set; }
    public decimal? CorrectLongitude { get; set; }
    public int? AcceptanceRadiusMeters { get; set; }
    public string? HintText { get; set; }
    public string? Explanation { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public QuestionBank? QuestionBank { get; set; }
    public Location? Location { get; set; }
    public ICollection<QuestionOption>? QuestionOptions { get; set; }
}