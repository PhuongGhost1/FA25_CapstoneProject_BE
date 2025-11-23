using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.QuestionBanks;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CusomMapOSM_Infrastructure.Features.QuestionBanks;

public class QuestionBankService : IQuestionBankService
{
    private readonly IQuestionBankRepository _questionBankRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public QuestionBankService(
        IQuestionBankRepository questionBankRepository,
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionBankRepository = questionBankRepository;
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<QuestionBankDTO, Error>> CreateQuestionBank(CreateQuestionBankRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var questionBank = new QuestionBank
        {
            QuestionBankId = Guid.NewGuid(),
            UserId = currentUserId.Value,
            WorkspaceId = request.WorkspaceId,
            MapId = request.MapId,
            BankName = request.BankName,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags,
            IsTemplate = request.IsTemplate,
            IsPublic = request.IsPublic,
            TotalQuestions = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _questionBankRepository.CreateQuestionBank(questionBank);
        if (!created)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Failure("QuestionBank.CreateFailed", "Failed to create question bank"));
        }

        return Option.Some<QuestionBankDTO, Error>(new QuestionBankDTO
        {
            QuestionBankId = questionBank.QuestionBankId,
            UserId = questionBank.UserId,
            UserName = string.Empty, // Will be populated by mapper
            WorkspaceId = questionBank.WorkspaceId,
            MapId = questionBank.MapId,
            BankName = questionBank.BankName,
            Description = questionBank.Description,
            Category = questionBank.Category,
            Tags = questionBank.Tags,
            TotalQuestions = questionBank.TotalQuestions,
            IsTemplate = questionBank.IsTemplate,
            IsPublic = questionBank.IsPublic,
            CreatedAt = questionBank.CreatedAt,
            UpdatedAt = questionBank.UpdatedAt
        });
    }

    public async Task<Option<QuestionBankDTO, Error>> GetQuestionBankById(Guid questionBankId)
    {
        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        return Option.Some<QuestionBankDTO, Error>(MapToQuestionBankDto(questionBank));
    }

    public async Task<Option<List<QuestionBankDTO>, Error>> GetMyQuestionBanks()
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<List<QuestionBankDTO>, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var questionBanks = await _questionBankRepository.GetQuestionBanksByUserId(currentUserId.Value);

        var dtos = questionBanks.Select(MapToQuestionBankDto).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(dtos);
    }

    public async Task<Option<List<QuestionBankDTO>, Error>> GetPublicQuestionBanks()
    {
        var questionBanks = await _questionBankRepository.GetPublicQuestionBanks();

        var dtos = questionBanks.Select(MapToQuestionBankDto).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(dtos);
    }

    public async Task<Option<bool, Error>> DeleteQuestionBank(Guid questionBankId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var owns = await _questionBankRepository.CheckUserOwnsQuestionBank(questionBankId, currentUserId.Value);
        if (!owns)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to delete this question bank"));
        }

        var deleted = await _questionBankRepository.DeleteQuestionBank(questionBankId);
        return deleted
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.Failure("QuestionBank.DeleteFailed", "Failed to delete question bank"));
    }

    public async Task<Option<Guid, Error>> CreateQuestion(CreateQuestionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<Guid, Error>(
                Error.Unauthorized("Question.Unauthorized", "User not authenticated"));
        }

        // Check if user owns the question bank
        var owns = await _questionBankRepository.CheckUserOwnsQuestionBank(request.QuestionBankId, currentUserId.Value);
        if (!owns)
        {
            return Option.None<Guid, Error>(
                Error.Forbidden("Question.NotOwner", "You don't have permission to add questions to this bank"));
        }

        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuestionBankId = request.QuestionBankId,
            LocationId = request.LocationId,
            QuestionType = request.QuestionType,
            QuestionText = request.QuestionText,
            QuestionImageUrl = request.QuestionImageUrl,
            QuestionAudioUrl = request.QuestionAudioUrl,
            Points = request.Points,
            TimeLimit = request.TimeLimit,
            CorrectAnswerText = request.CorrectAnswerText,
            CorrectLatitude = request.CorrectLatitude,
            CorrectLongitude = request.CorrectLongitude,
            AcceptanceRadiusMeters = request.AcceptanceRadiusMeters,
            HintText = request.HintText,
            Explanation = request.Explanation,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create options if provided and assign to question before saving
        if (request.Options != null && request.Options.Any())
        {
            var options = request.Options.Select(o => new QuestionOption
            {
                QuestionOptionId = Guid.NewGuid(),
                QuestionId = question.QuestionId,
                OptionText = o.OptionText,
                OptionImageUrl = o.OptionImageUrl,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            question.QuestionOptions = options;
        }

        var created = await _questionRepository.CreateQuestion(question);
        if (!created)
        {
            return Option.None<Guid, Error>(
                Error.Failure("Question.CreateFailed", "Failed to create question"));
        }

        // Update question count
        await _questionBankRepository.UpdateQuestionCount(request.QuestionBankId);

        return Option.Some<Guid, Error>(question.QuestionId);
    }

    public async Task<Option<List<QuestionDTO>, Error>> GetQuestionsByQuestionBankId(Guid questionBankId)
    {
        // Check if question bank exists
        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<List<QuestionDTO>, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        var questions = await _questionRepository.GetQuestionsWithOptions(questionBankId);
        var dtos = questions.Select(MapToQuestionDto).ToList();

        return Option.Some<List<QuestionDTO>, Error>(dtos);
    }

    public async Task<Option<QuestionBankDTO, Error>> AddQuestionBankTags(Guid questionBankId, UpdateQuestionBankTagsRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        if (questionBank.UserId != currentUserId.Value)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to modify this question bank"));
        }

        var normalizedTags = NormalizeTags(request.Tags);
        if (!normalizedTags.Any())
        {
            return Option.Some<QuestionBankDTO, Error>(MapToQuestionBankDto(questionBank));
        }

        var existingTags = ParseTags(questionBank.Tags);
        var added = false;
        foreach (var tag in normalizedTags)
        {
            if (!existingTags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                existingTags.Add(tag);
                added = true;
            }
        }

        if (!added)
        {
            return Option.Some<QuestionBankDTO, Error>(MapToQuestionBankDto(questionBank));
        }

        var serialized = SerializeTags(existingTags);
        if (serialized?.Length > 500)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.ValidationError("QuestionBank.TagsTooLong", "Combined tags exceed 500 characters"));
        }

        questionBank.Tags = serialized;
        var updated = await _questionBankRepository.UpdateQuestionBank(questionBank);
        if (!updated)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Failure("QuestionBank.UpdateFailed", "Failed to update tags"));
        }

        return Option.Some<QuestionBankDTO, Error>(MapToQuestionBankDto(questionBank));
    }

    public async Task<Option<QuestionBankDTO, Error>> ReplaceQuestionBankTags(Guid questionBankId, UpdateQuestionBankTagsRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        if (questionBank.UserId != currentUserId.Value)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to modify this question bank"));
        }

        var normalizedTags = NormalizeTags(request.Tags);
        var serialized = SerializeTags(normalizedTags);

        if (serialized?.Length > 500)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.ValidationError("QuestionBank.TagsTooLong", "Combined tags exceed 500 characters"));
        }

        questionBank.Tags = serialized;
        var updated = await _questionBankRepository.UpdateQuestionBank(questionBank);
        if (!updated)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Failure("QuestionBank.UpdateFailed", "Failed to update tags"));
        }

        return Option.Some<QuestionBankDTO, Error>(MapToQuestionBankDto(questionBank));
    }

    public async Task<Option<bool, Error>> DeleteQuestion(Guid questionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Question.Unauthorized", "User not authenticated"));
        }

        var question = await _questionRepository.GetQuestionById(questionId);
        if (question == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Question.NotFound", "Question not found"));
        }

        // Check if user owns the question bank
        var owns = await _questionBankRepository.CheckUserOwnsQuestionBank(question.QuestionBankId, currentUserId.Value);
        if (!owns)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Question.NotOwner", "You don't have permission to delete this question"));
        }

        var deleted = await _questionRepository.DeleteQuestion(questionId);
        if (!deleted)
        {
            return Option.None<bool, Error>(
                Error.Failure("Question.DeleteFailed", "Failed to delete question"));
        }

        // Update question count
        await _questionBankRepository.UpdateQuestionCount(question.QuestionBankId);

        return Option.Some<bool, Error>(true);
    }

    private static List<string> NormalizeTags(IEnumerable<string> tags)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (tags == null)
        {
            return result;
        }

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            var normalized = tag.Trim();
            if (normalized.Length == 0)
            {
                continue;
            }

            // Clamp each tag to 50 chars to avoid bloating the column.
            if (normalized.Length > 50)
            {
                normalized = normalized[..50];
            }

            if (seen.Add(normalized))
            {
                result.Add(normalized);
            }
        }

        return result;
    }

    private static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return new List<string>();
        }

        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => tag.Length > 0)
            .ToList();
    }

    private static string? SerializeTags(IReadOnlyCollection<string> tags)
    {
        if (tags.Count == 0)
        {
            return null;
        }

        var serialized = string.Join(",", tags);
        return serialized.Length == 0 ? null : serialized;
    }

    private static QuestionBankDTO MapToQuestionBankDto(QuestionBank questionBank)
    {
        return new QuestionBankDTO
        {
            QuestionBankId = questionBank.QuestionBankId,
            UserId = questionBank.UserId,
            UserName = questionBank.User?.FullName ?? string.Empty,
            WorkspaceId = questionBank.WorkspaceId,
            WorkspaceName = questionBank.Workspace?.WorkspaceName,
            MapId = questionBank.MapId,
            MapName = questionBank.Map?.MapName,
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

    private static QuestionDTO MapToQuestionDto(Question question)
    {
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
            Options = question.QuestionOptions?.Select(o => new QuestionOptionDTO
            {
                QuestionOptionId = o.QuestionOptionId,
                QuestionId = o.QuestionId,
                OptionText = o.OptionText,
                OptionImageUrl = o.OptionImageUrl,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder
            }).OrderBy(o => o.DisplayOrder).ToList()
        };
    }
}