namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;

public class QuestionBankDTO
{
    public Guid QuestionBankId { get; set; }
    public Guid UserId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}