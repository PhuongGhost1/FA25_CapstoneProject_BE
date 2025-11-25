using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class AttachQuestionBankToMapRequest
{
    [Required]
    public Guid MapId { get; set; }
}
