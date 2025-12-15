using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.QuestionBanks;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using Microsoft.AspNetCore.Http;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Infrastructure.Features.QuestionBanks;

public class QuestionBankService : IQuestionBankService
{
    private readonly IQuestionBankRepository _questionBankRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuestionOptionRepository  _questionOptionRepository;
    private readonly IMapRepository _mapRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionQuestionBankRepository _sessionQuestionBankRepository;
    private readonly ISessionQuestionRepository _sessionQuestionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly IUserAssetService _userAssetService;
    private readonly IWorkspaceRepository _workspaceRepository;

    public QuestionBankService(
        IQuestionBankRepository questionBankRepository,
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService, 
        IQuestionOptionRepository questionOptionRepository,
        IMapRepository mapRepository,
        ISessionRepository sessionRepository,
        ISessionQuestionBankRepository sessionQuestionBankRepository,
        ISessionQuestionRepository sessionQuestionRepository,
        IFirebaseStorageService firebaseStorageService,
        IUserAssetService userAssetService,
        IWorkspaceRepository workspaceRepository)
    {
        _questionBankRepository = questionBankRepository;
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
        _questionOptionRepository = questionOptionRepository;
        _mapRepository = mapRepository;
        _sessionRepository = sessionRepository;
        _sessionQuestionBankRepository = sessionQuestionBankRepository;
        _sessionQuestionRepository = sessionQuestionRepository;
        _firebaseStorageService = firebaseStorageService;
        _userAssetService = userAssetService;
        _workspaceRepository = workspaceRepository;
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
        return Option.Some<QuestionBankDTO, Error>(questionBank.ToDto());
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
        var result = questionBanks.Select(qb => qb.ToDto()).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(result);
    }

    public async Task<Option<List<QuestionBankDTO>, Error>> GetPublicQuestionBanks()
    {
        var questionBanks = await _questionBankRepository.GetPublicQuestionBanks();
        var result = questionBanks.Select(qb => qb.ToDto()).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(result);
    }

    public async Task<Option<List<QuestionBankDTO>, Error>> GetMyQuestionBanksByOrganization(Guid orgId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<List<QuestionBankDTO>, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        // Get all workspaces in this organization
        var workspaces = await _workspaceRepository.GetByOrganizationIdAsync(orgId);
        var workspaceIds = workspaces.Select(w => w.WorkspaceId).ToList();

        if (!workspaceIds.Any())
        {
            return Option.Some<List<QuestionBankDTO>, Error>(new List<QuestionBankDTO>());
        }

        // Get question banks created by this user in these workspaces
        var questionBanks = await _questionBankRepository.GetQuestionBanksByUserIdAndWorkspaceIds(currentUserId.Value, workspaceIds);
        var result = questionBanks.Select(qb => qb.ToDto()).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(result);
    }

    public async Task<Option<List<QuestionBankDTO>, Error>> GetPublicQuestionBanksByOrganization(Guid orgId)
    {
        // Get all workspaces in this organization
        var workspaces = await _workspaceRepository.GetByOrganizationIdAsync(orgId);
        var workspaceIds = workspaces.Select(w => w.WorkspaceId).ToList();

        if (!workspaceIds.Any())
        {
            return Option.Some<List<QuestionBankDTO>, Error>(new List<QuestionBankDTO>());
        }

        // Get public question banks in these workspaces
        var questionBanks = await _questionBankRepository.GetPublicQuestionBanksByWorkspaceIds(workspaceIds);
        var result = questionBanks.Select(qb => qb.ToDto()).ToList();

        return Option.Some<List<QuestionBankDTO>, Error>(result);
    }

    public async Task<Option<QuestionBankDTO, Error>> DuplicateQuestionBank(Guid questionBankId, Guid targetWorkspaceId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        // Get the source question bank
        var sourceBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (sourceBank == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        // Verify target workspace exists
        var targetWorkspace = await _workspaceRepository.GetByIdAsync(targetWorkspaceId);
        if (targetWorkspace == null)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.NotFound("Workspace.NotFound", "Target workspace not found"));
        }

        // Create new question bank as a copy
        var newBank = new QuestionBank
        {
            QuestionBankId = Guid.NewGuid(),
            UserId = currentUserId.Value,
            WorkspaceId = targetWorkspaceId,
            BankName = $"{sourceBank.BankName} (Copy)",
            Description = sourceBank.Description,
            Category = sourceBank.Category,
            Tags = sourceBank.Tags,
            IsTemplate = false, // New copy is not a template
            IsPublic = false,   // New copy is private
            IsActive = true,
            TotalQuestions = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _questionBankRepository.CreateQuestionBank(newBank);
        if (!created)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Failure("QuestionBank.DuplicateFailed", "Failed to create duplicate question bank"));
        }

        // Copy all questions from source bank
        var sourceQuestions = await _questionRepository.GetQuestionsByQuestionBankId(questionBankId);
        foreach (var sourceQuestion in sourceQuestions)
        {
            var newQuestion = new Question
            {
                QuestionId = Guid.NewGuid(),
                QuestionBankId = newBank.QuestionBankId,
                LocationId = sourceQuestion.LocationId,
                QuestionType = sourceQuestion.QuestionType,
                QuestionText = sourceQuestion.QuestionText,
                QuestionImageUrl = sourceQuestion.QuestionImageUrl,
                QuestionAudioUrl = sourceQuestion.QuestionAudioUrl,
                Points = sourceQuestion.Points,
                TimeLimit = sourceQuestion.TimeLimit,
                CorrectAnswerText = sourceQuestion.CorrectAnswerText,
                CorrectLatitude = sourceQuestion.CorrectLatitude,
                CorrectLongitude = sourceQuestion.CorrectLongitude,
                AcceptanceRadiusMeters = sourceQuestion.AcceptanceRadiusMeters,
                HintText = sourceQuestion.HintText,
                Explanation = sourceQuestion.Explanation,
                DisplayOrder = sourceQuestion.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _questionRepository.CreateQuestion(newQuestion);

            // Copy options for this question
            var sourceOptions = await _questionOptionRepository.GetQuestionOptionsByQuestionId(sourceQuestion.QuestionId);
            foreach (var sourceOption in sourceOptions)
            {
                var newOption = new QuestionOption
                {
                    QuestionOptionId = Guid.NewGuid(),
                    QuestionId = newQuestion.QuestionId,
                    OptionText = sourceOption.OptionText,
                    OptionImageUrl = sourceOption.OptionImageUrl,
                    IsCorrect = sourceOption.IsCorrect,
                    DisplayOrder = sourceOption.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                await _questionOptionRepository.CreateQuestionOption(newOption);
            }
        }

        // Update question count
        await _questionBankRepository.UpdateQuestionCount(newBank.QuestionBankId);

        // Reload to get updated data
        var result = await _questionBankRepository.GetQuestionBankById(newBank.QuestionBankId);
        return Option.Some<QuestionBankDTO, Error>(result!.ToDto());
    }

    public async Task<Option<QuestionBankDTO, Error>> UpdateQuestionBank(Guid questionBankId, UpdateQuestionBankRequest request)
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

        questionBank.BankName = request.BankName;
        questionBank.Description = request.Description;
        questionBank.Category = request.Category;
        questionBank.Tags = request.Tags;
        questionBank.IsTemplate = request.IsTemplate;
        questionBank.IsPublic = request.IsPublic;
        questionBank.UpdatedAt = DateTime.UtcNow;

        var updated = await _questionBankRepository.UpdateQuestionBank(questionBank);
        if (!updated)
        {
            return Option.None<QuestionBankDTO, Error>(
                Error.Failure("QuestionBank.UpdateFailed", "Failed to update question bank"));
        }

        return Option.Some<QuestionBankDTO, Error>(questionBank.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteQuestionBank(Guid questionBankId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        if (questionBank.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to modify this question bank"));
        }

        // Check if question bank is being used in active sessions
        var sessionsUsingBank = await _sessionQuestionBankRepository.GetSessions(questionBankId);
        
        // Filter for actual active sessions (not null session reference and active status)
        var activeSessions = sessionsUsingBank
            .Where(sqb => sqb.Session != null && 
                (sqb.Session.Status == SessionStatusEnum.WAITING || 
                 sqb.Session.Status == SessionStatusEnum.IN_PROGRESS || 
                 sqb.Session.Status == SessionStatusEnum.PAUSED))
            .ToList();
        
        if (activeSessions.Any())
        {
            var sessionNames = string.Join(", ", activeSessions
                .Select(s => s.Session!.SessionName)
                .Take(3));
            return Option.None<bool, Error>(
                Error.ValidationError("QuestionBank.HasActiveSessions", 
                    $"Không thể xóa ngân hàng câu hỏi khi đang được sử dụng trong phiên hoạt động: {sessionNames}. Vui lòng kết thúc hoặc hủy tất cả phiên hoạt động trước."));
        }
        
        // Clean up orphan SessionQuestionBank records (sessions that no longer exist)
        var orphanRecords = sessionsUsingBank.Where(sqb => sqb.Session == null).ToList();
        foreach (var orphan in orphanRecords)
        {
            await _sessionQuestionBankRepository.RemoveQuestionBank(orphan);
        }

        await _questionRepository.DeleteQuestionsByBankId(questionBankId);

        var deleted = await _questionBankRepository.DeleteQuestionBank(questionBankId);
        if (!deleted)
        {
            return Option.None<bool, Error>(
                Error.Failure("QuestionBank.DeleteFailed", "Failed to delete question bank"));
        }

        return Option.Some<bool, Error>(true);
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

        var existingTags = ParseTags(questionBank.Tags);
        var incomingTags = NormalizeTags(request.Tags);

        foreach (var tag in incomingTags)
        {
            if (!existingTags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                existingTags.Add(tag);
            }
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

        return Option.Some<QuestionBankDTO, Error>(questionBank.ToDto());
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

        return Option.Some<QuestionBankDTO, Error>(questionBank.ToDto());
    }

    public async Task<Option<Guid, Error>> CreateQuestion(CreateQuestionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<Guid, Error>(
                Error.Unauthorized("Question.Unauthorized", "User not authenticated"));
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(request.QuestionBankId);
        if (questionBank == null)
        {
            return Option.None<Guid, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        if (questionBank.UserId != currentUserId.Value)
        {
            return Option.None<Guid, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to modify this question bank"));
        }

        if (request.QuestionType is QuestionTypeEnum.MULTIPLE_CHOICE or QuestionTypeEnum.TRUE_FALSE)
        {
            if (request.Options == null || !request.Options.Any())
            {
                return Option.None<Guid, Error>(
                    Error.ValidationError("Question.OptionsRequired", "Options are required for this question type"));
            }

            if (!request.Options.Any(o => o.IsCorrect))
            {
                return Option.None<Guid, Error>(
                    Error.ValidationError("Question.NoCorrectOption", "At least one option must be marked correct"));
            }
        }

        if (request.QuestionType == QuestionTypeEnum.SHORT_ANSWER &&
            string.IsNullOrWhiteSpace(request.CorrectAnswerText))
        {
            return Option.None<Guid, Error>(
                Error.ValidationError("Question.InvalidAnswer", "Short answer questions require a correct answer"));
        }

        if (request.QuestionType == QuestionTypeEnum.PIN_ON_MAP &&
            (request.CorrectLatitude == null || request.CorrectLongitude == null || request.AcceptanceRadiusMeters == null))
        {
            return Option.None<Guid, Error>(
                Error.ValidationError("Question.InvalidCoordinates", "Pin on map questions require coordinates and a radius"));
        }

        var questionId = Guid.NewGuid();
        var question = new Question
        {
            QuestionId = questionId,
            QuestionBankId = request.QuestionBankId,
            LocationId = request.LocationId,
            QuestionType = request.QuestionType,
            QuestionText = request.QuestionText,
            QuestionImageUrl = request.QuestionImageUrl,
            QuestionAudioUrl = request.QuestionAudioUrl,
            Points = request.Points,
            TimeLimit = request.TimeLimit,
            HintText = request.HintText,
            Explanation = request.Explanation,
            DisplayOrder = request.DisplayOrder,
            CorrectAnswerText = request.QuestionType == QuestionTypeEnum.SHORT_ANSWER ? request.CorrectAnswerText : null,
            CorrectLatitude = request.QuestionType == QuestionTypeEnum.PIN_ON_MAP ? request.CorrectLatitude : null,
            CorrectLongitude = request.QuestionType == QuestionTypeEnum.PIN_ON_MAP ? request.CorrectLongitude : null,
            AcceptanceRadiusMeters = request.QuestionType == QuestionTypeEnum.PIN_ON_MAP ? request.AcceptanceRadiusMeters : null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _questionRepository.CreateQuestion(question);
        if (!created)
        {
            return Option.None<Guid, Error>(
                Error.Failure("Question.CreateFailed", "Failed to create question"));
        }

        if (request.QuestionType is QuestionTypeEnum.MULTIPLE_CHOICE or QuestionTypeEnum.TRUE_FALSE)
        {
            var options = request.Options!
                .Select(o => new QuestionOption
                {
                    QuestionOptionId = Guid.NewGuid(),
                    QuestionId = questionId,
                    OptionText = o.OptionText,
                    OptionImageUrl = o.OptionImageUrl,
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            var createdOptions = await _questionOptionRepository.CreateQuestionOptions(options);
            if (!createdOptions)
            {
                return Option.None<Guid, Error>(
                    Error.Failure("QuestionOption.CreateFailed", "Failed to create question options"));
            }
        }

        await _questionBankRepository.UpdateQuestionCount(request.QuestionBankId);

        return Option.Some<Guid, Error>(questionId);
    }

    public async Task<Option<List<QuestionDTO>, Error>> GetQuestionsByQuestionBankId(Guid questionBankId)
    {
        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<List<QuestionDTO>, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        var questions = await _questionRepository.GetQuestionsByQuestionBankId(questionBankId);
        var result = new List<QuestionDTO>();

        foreach (var question in questions)
        {
            var options = await _questionOptionRepository.GetQuestionOptionsByQuestionId(question.QuestionId);
            result.Add(question.ToDto(options));
        }

        return Option.Some<List<QuestionDTO>, Error>(result);
    }

    public async Task<Option<Guid, Error>> UpdateQuestion(UpdateQuestionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<Guid, Error>(
                Error.Unauthorized("Question.Unauthorized", "User not authenticated"));
        }

        var question = await _questionRepository.GetQuestionById(request.QuestionId);
        if (question == null)
        {
            return Option.None<Guid, Error>(
                Error.NotFound("Question.NotFound", "Question not found"));
        }

        var owns = await _questionBankRepository.CheckUserOwnsQuestionBank(question.QuestionBankId, currentUserId.Value);
        if (!owns)
        {
            return Option.None<Guid, Error>(
                Error.Forbidden("Question.NotOwner", "You don't have permission to update this question"));
        }

        if (request.QuestionType is QuestionTypeEnum.MULTIPLE_CHOICE or QuestionTypeEnum.TRUE_FALSE)
        {
            if (request.Options == null || !request.Options.Any())
            {
                return Option.None<Guid, Error>(
                    Error.ValidationError("Question.OptionsRequired", "Options are required for this question type"));
            }

            if (!request.Options.Any(o => o.IsCorrect))
            {
                return Option.None<Guid, Error>(
                    Error.ValidationError("Question.NoCorrectOption", "At least one option must be marked correct"));
            }
        }

        if (request.QuestionType == QuestionTypeEnum.SHORT_ANSWER &&
            string.IsNullOrWhiteSpace(request.CorrectAnswerText))
        {
            return Option.None<Guid, Error>(
                Error.ValidationError("Question.InvalidAnswer", "Short answer questions require a correct answer"));
        }

        if (request.QuestionType == QuestionTypeEnum.PIN_ON_MAP &&
            (request.CorrectLatitude == null || request.CorrectLongitude == null || request.AcceptanceRadiusMeters == null))
        {
            return Option.None<Guid, Error>(
                Error.ValidationError("Question.InvalidCoordinates", "Pin on map questions require coordinates and a radius"));
        }

        question.QuestionText = request.QuestionText;
        question.QuestionType = request.QuestionType;
        question.LocationId = request.LocationId;
        question.Points = request.Points;
        question.TimeLimit = request.TimeLimit ?? question.TimeLimit;
        question.QuestionImageUrl = request.QuestionImageUrl;
        question.QuestionAudioUrl = request.QuestionAudioUrl;
        question.HintText = request.HintText;
        question.Explanation = request.Explanation;
        question.DisplayOrder = request.DisplayOrder;

        switch (request.QuestionType)
        {
            case QuestionTypeEnum.MULTIPLE_CHOICE:
            case QuestionTypeEnum.TRUE_FALSE:
                question.CorrectAnswerText = null;
                question.CorrectLatitude = null;
                question.CorrectLongitude = null;
                question.AcceptanceRadiusMeters = null;
                break;
            case QuestionTypeEnum.SHORT_ANSWER:
                question.CorrectAnswerText = request.CorrectAnswerText;
                question.CorrectLatitude = null;
                question.CorrectLongitude = null;
                question.AcceptanceRadiusMeters = null;
                break;
            case QuestionTypeEnum.PIN_ON_MAP:
                question.CorrectLatitude = request.CorrectLatitude;
                question.CorrectLongitude = request.CorrectLongitude;
                question.AcceptanceRadiusMeters = request.AcceptanceRadiusMeters;
                question.CorrectAnswerText = null;
                break;
        }

        var updated = await _questionRepository.UpdateQuestion(question);
        if (!updated)
        {
            return Option.None<Guid, Error>(
                Error.Failure("Question.UpdateFailed", "Failed to update question"));
        }

        var optionsSynced = await SyncQuestionOptionsForUpdate(question.QuestionId, request.QuestionType, request.Options);
        if (!optionsSynced)
        {
            return Option.None<Guid, Error>(
                Error.Failure("QuestionOption.UpdateFailed", "Failed to update question options"));
        }

        return Option.Some<Guid, Error>(request.QuestionId);
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

        // Check if question is being used in active sessions
        var sessionQuestions = await _sessionQuestionRepository.GetQuestionsByQuestionId(questionId);
        var activeSessions = sessionQuestions
            .Where(sq => sq.Session != null && 
                (sq.Session.Status == SessionStatusEnum.WAITING || 
                 sq.Session.Status == SessionStatusEnum.IN_PROGRESS || 
                 sq.Session.Status == SessionStatusEnum.PAUSED))
            .ToList();
        
        if (activeSessions.Any())
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Question.HasActiveSessions", 
                    "Cannot delete question while it is being used in active sessions. Please end or cancel all active sessions first."));
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

    public async Task<Option<bool, Error>> AttachQuestionBankToSession(Guid questionBankId, AttachQuestionBankToSessionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var session = await _sessionRepository.GetSessionById(request.SessionId);
        if (session == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        if (session.HostUserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "You don't have permission to modify this session"));
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        if (questionBank.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("QuestionBank.NotOwner", "You don't have permission to attach this question bank"));
        }

        var alreadyAttached = await _sessionQuestionBankRepository.CheckQuestionBankAttached(request.SessionId, questionBankId);
        if (alreadyAttached)
        {
            return Option.Some<bool, Error>(true);
        }

        var sessionQuestionBank = new SessionQuestionBank
        {
            SessionQuestionBankId = Guid.NewGuid(),
            SessionId = request.SessionId,
            QuestionBankId = questionBankId,
            AttachedAt = DateTime.UtcNow
        };

        var attached = await _sessionQuestionBankRepository.AddQuestionBank(sessionQuestionBank);
        if (!attached)
        {
            return Option.None<bool, Error>(
                Error.Failure("SessionQuestionBank.AttachFailed", "Failed to attach question bank to session"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> DetachQuestionBankFromSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("QuestionBank.Unauthorized", "User not authenticated"));
        }

        var session = await _sessionRepository.GetSessionById(sessionId);
        if (session == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        if (session.HostUserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "You don't have permission to modify this session"));
        }

        var assignments = await _sessionQuestionBankRepository.GetQuestionBanks(sessionId);
        if (assignments.Count == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("SessionQuestionBank.NotFound", "No question banks attached to this session"));
        }

        foreach (var assignment in assignments)
        {
            var removed = await _sessionQuestionBankRepository.RemoveQuestionBank(assignment);
            if (!removed)
            {
                return Option.None<bool, Error>(
                    Error.Failure("SessionQuestionBank.DetachFailed", "Failed to detach question bank from session"));
            }
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<List<QuestionDTO>, Error>> GetQuestionBanksBySessionId(Guid sessionId)
    {
        var session = await _sessionRepository.GetSessionById(sessionId);
        if (session == null)
        {
            return Option.None<List<QuestionDTO>, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        var assignments = await _sessionQuestionBankRepository.GetQuestionBanks(sessionId);
        if (assignments.Count == 0)
        {
            return Option.Some<List<QuestionDTO>, Error>(new List<QuestionDTO>());
        }

        var questions = new List<QuestionDTO>();

        foreach (var assignment in assignments)
        {
            var bankQuestions = await _questionRepository.GetQuestionsByQuestionBankId(assignment.QuestionBankId);
            foreach (var question in bankQuestions)
            {
                var options = await _questionOptionRepository.GetQuestionOptionsByQuestionId(question.QuestionId);
                questions.Add(question.ToDto(options));
            }
        }

        return Option.Some<List<QuestionDTO>, Error>(questions);
    }

    public async Task<Option<List<SessionQuestionBankResponse>, Error>> GetSessionsByQuestionBankId(Guid questionBankId)
    {
        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
        if (questionBank == null)
        {
            return Option.None<List<SessionQuestionBankResponse>, Error>(
                Error.NotFound("QuestionBank.NotFound", "Question bank not found"));
        }

        var assignments = await _sessionQuestionBankRepository.GetSessions(questionBankId);
        if (assignments.Count == 0)
        {
            return Option.Some<List<SessionQuestionBankResponse>, Error>(new List<SessionQuestionBankResponse>());
        }

        var sessions = assignments
            .Where(x => x.Session != null)
            .Select(x => new SessionQuestionBankResponse
            {
                SessionId = x.SessionId,
                SessionName = x.Session.SessionName,
                SessionCode = x.Session.SessionCode,
                AttachedAt = x.AttachedAt
            })
            .ToList();

        return Option.Some<List<SessionQuestionBankResponse>, Error>(sessions);
    }

    private async Task<bool> SyncQuestionOptionsForUpdate(
        Guid questionId,
        QuestionTypeEnum questionType,
        List<UpdateQuestionOptionRequest>? requestedOptions)
    {
        var existingOptions = await _questionOptionRepository.GetQuestionOptionsByQuestionId(questionId);

        if (questionType is QuestionTypeEnum.MULTIPLE_CHOICE or QuestionTypeEnum.TRUE_FALSE)
        {
            var incomingIds = requestedOptions?
                .Where(o => o.QuestionOptionId.HasValue)
                .Select(o => o.QuestionOptionId!.Value)
                .ToHashSet() ?? new HashSet<Guid>();

            foreach (var option in existingOptions)
            {
                if (!incomingIds.Contains(option.QuestionOptionId))
                {
                    var deleted = await _questionOptionRepository.DeleteQuestionOption(option.QuestionOptionId);
                    if (!deleted)
                    {
                        return false;
                    }
                }
            }

            foreach (var option in requestedOptions!)
            {
                if (option.QuestionOptionId.HasValue)
                {
                    var updated = await _questionOptionRepository.UpdateQuestionOption(new QuestionOption
                    {
                        QuestionOptionId = option.QuestionOptionId.Value,
                        QuestionId = questionId,
                        OptionText = option.OptionText,
                        OptionImageUrl = option.OptionImageUrl,
                        IsCorrect = option.IsCorrect,
                        DisplayOrder = option.DisplayOrder
                    });

                    if (!updated)
                    {
                        return false;
                    }
                }
                else
                {
                    var created = await _questionOptionRepository.CreateQuestionOption(new QuestionOption
                    {
                        QuestionOptionId = Guid.NewGuid(),
                        QuestionId = questionId,
                        OptionText = option.OptionText,
                        OptionImageUrl = option.OptionImageUrl,
                        IsCorrect = option.IsCorrect,
                        DisplayOrder = option.DisplayOrder,
                        CreatedAt = DateTime.UtcNow
                    });

                    if (!created)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        foreach (var option in existingOptions)
        {
            var deleted = await _questionOptionRepository.DeleteQuestionOption(option.QuestionOptionId);
            if (!deleted)
            {
                return false;
            }
        }

        return true;
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

    private async Task<Guid?> GetOrganizationIdAsync(Guid? questionBankId)
    {
        if (!questionBankId.HasValue)
        {
            return null;
        }

        var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId.Value);
        if (questionBank == null || !questionBank.WorkspaceId.HasValue)
        {
            return null;
        }

        var workspace = await _workspaceRepository.GetByIdAsync(questionBank.WorkspaceId.Value);
        return workspace?.OrgId;
    }

    public async Task<Option<string, Error>> UploadQuestionImage(IFormFile file, Guid? questionBankId)
    {
        if (file == null || file.Length == 0)
        {
            return Option.None<string, Error>(Error.ValidationError("File.Empty", "No file provided"));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Option.None<string, Error>(Error.ValidationError("File.InvalidType", "Invalid file type. Only images are allowed."));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var storageUrl = await _firebaseStorageService.UploadFileAsync(file.FileName, stream, "question-images");
            
            // Register in User Library
            var userId = _currentUserService.GetUserId();
            if (userId.HasValue)
            {
                var orgId = await GetOrganizationIdAsync(questionBankId);
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        file.FileName,
                        storageUrl,
                        file.ContentType,
                        file.Length,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }

            return Option.Some<string, Error>(storageUrl);
        }
        catch (Exception ex)
        {
            return Option.None<string, Error>(Error.Failure("File.UploadFailed", ex.Message));
        }
    }

    public async Task<Option<string, Error>> UploadQuestionAudio(IFormFile file, Guid? questionBankId)
    {
        if (file == null || file.Length == 0)
        {
            return Option.None<string, Error>(Error.ValidationError("File.Empty", "No file provided"));
        }

        var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Option.None<string, Error>(Error.ValidationError("File.InvalidType", "Invalid file type. Only audio files are allowed."));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var storageUrl = await _firebaseStorageService.UploadFileAsync(file.FileName, stream, "question-audio");
            
            // Register in User Library
            var userId = _currentUserService.GetUserId();
            if (userId.HasValue)
            {
                var orgId = await GetOrganizationIdAsync(questionBankId);
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        file.FileName,
                        storageUrl,
                        file.ContentType,
                        file.Length,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }

            return Option.Some<string, Error>(storageUrl);
        }
        catch (Exception ex)
        {
            return Option.None<string, Error>(Error.Failure("File.UploadFailed", ex.Message));
        }
    }

    public async Task<Option<string, Error>> UploadOptionImage(IFormFile file, Guid? questionBankId)
    {
        if (file == null || file.Length == 0)
        {
            return Option.None<string, Error>(Error.ValidationError("File.Empty", "No file provided"));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Option.None<string, Error>(Error.ValidationError("File.InvalidType", "Invalid file type. Only images are allowed."));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var storageUrl = await _firebaseStorageService.UploadFileAsync(file.FileName, stream, "question-option-images");
            
            // Register in User Library
            var userId = _currentUserService.GetUserId();
            if (userId.HasValue)
            {
                var orgId = await GetOrganizationIdAsync(questionBankId);
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        file.FileName,
                        storageUrl,
                        file.ContentType,
                        file.Length,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }

            return Option.Some<string, Error>(storageUrl);
        }
        catch (Exception ex)
        {
            return Option.None<string, Error>(Error.Failure("File.UploadFailed", ex.Message));
        }
    }
    
}