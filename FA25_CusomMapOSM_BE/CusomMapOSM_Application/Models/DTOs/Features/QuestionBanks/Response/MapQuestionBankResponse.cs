namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;

public class MapQuestionBankResponse
{
    public Guid MapId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

