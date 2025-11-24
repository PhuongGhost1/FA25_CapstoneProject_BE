using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Application.Common.Mappers;

public static class QuestionBankMapping
{
    public static QuestionBankDTO ToDto(this QuestionBank questionBank)
    {
        return new QuestionBankDTO
        {
            QuestionBankId = questionBank.QuestionBankId,
            UserId = questionBank.UserId,
            WorkspaceId = questionBank.WorkspaceId,
            BankName = questionBank.BankName,
            Description = questionBank.Description,
            Category = questionBank.Category,
            Tags = questionBank.Tags,
            TotalQuestions = questionBank.TotalQuestions,
            IsTemplate = questionBank.IsTemplate,
            IsPublic = questionBank.IsPublic,
            CreatedAt = questionBank.CreatedAt,
            UpdatedAt = questionBank.UpdatedAt
        };
    }
}