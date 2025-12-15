using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class AttachQuestionBankToSessionRequest
{
    [Required]
    public Guid SessionId { get; set; }
}

