namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;

public class SessionQuestionBankResponse
{
    public Guid SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public DateTime AttachedAt { get; set; }
}

