using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Application.Common.Mappers;

public static class QuestionMapping
{
    public static QuestionDTO ToDto(this Question question, IEnumerable<QuestionOption>? options = null)
    {
        var optionDtos = options?
            .OrderBy(o => o.DisplayOrder)
            .Select(o => new QuestionOptionDTO
            {
                QuestionOptionId = o.QuestionOptionId,
                QuestionId = o.QuestionId,
                OptionText = o.OptionText,
                OptionImageUrl = o.OptionImageUrl,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder
            })
            .ToList();

        return new QuestionDTO
        {
            QuestionId = question.QuestionId,
            QuestionBankId = question.QuestionBankId,
            LocationId = question.LocationId,
            QuestionType = question.QuestionType.ToString(),
            QuestionText = question.QuestionText,
            QuestionImageUrl = question.QuestionImageUrl,
            QuestionAudioUrl = question.QuestionAudioUrl,
            Points = question.Points,
            TimeLimit = question.TimeLimit,
            CorrectAnswerText = question.CorrectAnswerText,
            CorrectLatitude = question.CorrectLatitude,
            CorrectLongitude = question.CorrectLongitude,
            AcceptanceRadiusMeters = question.AcceptanceRadiusMeters,
            HintText = question.HintText,
            Explanation = question.Explanation,
            DisplayOrder = question.DisplayOrder,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Options = optionDtos
        };
    }
}