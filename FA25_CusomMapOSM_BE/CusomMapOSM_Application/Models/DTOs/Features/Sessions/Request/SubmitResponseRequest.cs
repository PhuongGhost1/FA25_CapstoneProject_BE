using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;

public class SubmitResponseRequest
{
    [Required]
    public Guid SessionQuestionId { get; set; }

    // For MULTIPLE_CHOICE and TRUE_FALSE
    public Guid? QuestionOptionId { get; set; }

    // For SHORT_ANSWER
    [StringLength(1000)]
    public string? ResponseText { get; set; }

    // For PIN_ON_MAP
    public decimal? ResponseLatitude { get; set; }
    public decimal? ResponseLongitude { get; set; }

    public decimal ResponseTimeSeconds { get; set; }
    public bool UsedHint { get; set; } = false;
}