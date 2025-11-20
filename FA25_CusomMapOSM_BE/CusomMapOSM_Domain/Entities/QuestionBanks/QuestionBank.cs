using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Workspaces;

namespace CusomMapOSM_Domain.Entities.QuestionBanks;

public class QuestionBank
{
    public Guid QuestionBankId { get; set; }
    public Guid UserId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public Guid? MapId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int TotalQuestions { get; set; } = 0;
    public bool IsTemplate { get; set; } = false;
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Workspace? Workspace { get; set; }
    public Map? Map { get; set; }
    public ICollection<Question>? Questions { get; set; }
}