using System.ComponentModel.DataAnnotations;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class UpdateQuestionRequest
{
    [Required]
    public Guid QuestionId { get; set; }

    public Guid? LocationId { get; set; }

    [Required]
    public QuestionTypeEnum QuestionType { get; set; }

    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? QuestionImageUrl { get; set; }
    public string? QuestionAudioUrl { get; set; }

    [Range(1, 1000)]
    public int Points { get; set; } = 100;

    [Range(5, 300)]
    public int? TimeLimit { get; set; }

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
    public List<UpdateQuestionOptionRequest>? Options { get; set; }
}

public class UpdateQuestionOptionRequest
{
    public Guid? QuestionOptionId { get; set; } // If null, create new option

    [Required]
    [MaxLength(500)]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; } = false;

    public string? OptionImageUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;
}

