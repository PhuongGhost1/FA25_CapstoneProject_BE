using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Domain.Entities.QuestionBanks;

public class MapQuestionBank
{
    public Guid MapId { get; set; }
    public Guid QuestionBankId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Map Map { get; set; } = null!;
    public QuestionBank QuestionBank { get; set; } = null!;
}
