using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Domain.Entities.Sessions;

public class SessionQuestionBank
{
    public Guid SessionQuestionBankId { get; set; }
    public Guid SessionId { get; set; }
    public Guid QuestionBankId { get; set; }
    public DateTime AttachedAt { get; set; } = DateTime.UtcNow;
    public Session? Session { get; set; }
    public QuestionBank? QuestionBank { get; set; }
}

