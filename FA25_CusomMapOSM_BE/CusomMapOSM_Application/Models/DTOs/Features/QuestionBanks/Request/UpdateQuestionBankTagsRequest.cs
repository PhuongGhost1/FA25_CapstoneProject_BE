namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class UpdateQuestionBankTagsRequest
{
    /// <summary>
    /// Danh sách tag mới. Chuỗi trống hoặc null sẽ bị loại bỏ tự động.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

