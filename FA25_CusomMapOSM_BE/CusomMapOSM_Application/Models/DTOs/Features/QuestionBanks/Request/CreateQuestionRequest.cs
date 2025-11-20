using System.ComponentModel.DataAnnotations;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class CreateQuestionRequest
{
    [Required]
    public Guid QuestionBankId { get; set; }

    public Guid? LocationId { get; set; }

    [Required]
    public QuestionTypeEnum QuestionType { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public string? QuestionImageUrl { get; set; }
    public string? QuestionAudioUrl { get; set; }

    public int Points { get; set; } = 100;
    public int TimeLimit { get; set; } = 30;

    // For SHORT_ANSWER
    public string? CorrectAnswerText { get; set; }

    // For PIN_ON_MAP
    public decimal? CorrectLatitude { get; set; }
    public decimal? CorrectLongitude { get; set; }
    public int? AcceptanceRadiusMeters { get; set; }

    public string? HintText { get; set; }
    public string? Explanation { get; set; }
    public int DisplayOrder { get; set; } = 0;

    // For MULTIPLE_CHOICE and TRUE_FALSE
    public List<CreateQuestionOptionRequest>? Options { get; set; }
}

public class CreateQuestionOptionRequest
{
    [Required]
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
    public bool IsCorrect { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}