using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.QuickPolls;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Response;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.QuickPolls;

public class QuickPollService : IQuickPollService
{
    private const string POLL_KEY_PREFIX = "poll:";
    private const string VOTE_KEY_PREFIX = "poll:votes:";
    private const string ACTIVE_POLL_KEY_PREFIX = "poll:active:";
    private const string SESSION_POLLS_KEY_PREFIX = "poll:session:list:";
    private static readonly TimeSpan PollPersistenceTtl = TimeSpan.FromDays(30);

    private readonly IRedisCacheService _cacheService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly JsonSerializerOptions _serializerOptions;

    public QuickPollService(
        IRedisCacheService cacheService,
        ISessionRepository sessionRepository,
        ICurrentUserService currentUserService)
    {
        _cacheService = cacheService;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _serializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<Option<QuickPollResponse, Error>> CreatePoll(CreateQuickPollRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<QuickPollResponse, Error>(
                Error.Unauthorized("QuickPoll.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(request.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<QuickPollResponse, Error>(
                Error.Forbidden("QuickPoll.NotHost", "Only session host can create polls"));
        }

        var validationError = ValidatePollRequest(request);
        if (validationError != null)
        {
            return Option.None<QuickPollResponse, Error>(validationError);
        }

        var pollId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var pollData = BuildPollData(pollId, request, createdAt);

        await SavePollData(pollData);
        await AppendPollToHistory(pollData.SessionId, pollId);

        if (pollData.Status == PollStatusEnum.Active)
        {
            await SetActivePoll(pollData.SessionId, pollId);
        }

        var response = await BuildPollResponse(pollData);
        return Option.Some<QuickPollResponse, Error>(response);
    }

    public async Task<Option<QuickPollResponse, Error>> GetActivePoll(Guid sessionId)
    {
        var pollIdStr = await _cacheService.Get<string>($"{ACTIVE_POLL_KEY_PREFIX}{sessionId}");
        if (string.IsNullOrWhiteSpace(pollIdStr) || !Guid.TryParse(pollIdStr, out var pollId))
        {
            return Option.None<QuickPollResponse, Error>(
                Error.NotFound("QuickPoll.NoActivePoll", "No active poll found for this session"));
        }

        var pollData = await GetPollData(pollId);
        if (pollData == null || pollData.Status != PollStatusEnum.Active)
        {
            return Option.None<QuickPollResponse, Error>(
                Error.NotFound("QuickPoll.NoActivePoll", "No active poll found for this session"));
        }

        return Option.Some<QuickPollResponse, Error>(await BuildPollResponse(pollData));
    }

    public async Task<Option<QuickPollResponse, Error>> GetPollResults(Guid pollId)
    {
        var pollData = await GetPollData(pollId);
        if (pollData == null)
        {
            return Option.None<QuickPollResponse, Error>(
                Error.NotFound("QuickPoll.NotFound", "Poll not found or expired"));
        }

        return Option.Some<QuickPollResponse, Error>(await BuildPollResponse(pollData));
    }

    public async Task<Option<IReadOnlyList<QuickPollResponse>, Error>> GetPollHistory(Guid sessionId)
    {
        var historyKey = $"{SESSION_POLLS_KEY_PREFIX}{sessionId}";
        var pollIds = await _cacheService.Get<List<Guid>>(historyKey) ?? new List<Guid>();
        var result = new List<QuickPollResponse>();

        foreach (var pollId in pollIds)
        {
            var pollData = await GetPollData(pollId);
            if (pollData != null)
            {
                result.Add(await BuildPollResponse(pollData));
            }
        }

        if (result.Count == 0)
        {
            return Option.None<IReadOnlyList<QuickPollResponse>, Error>(
                Error.NotFound("QuickPoll.HistoryEmpty", "No polls found for this session"));
        }

        return Option.Some<IReadOnlyList<QuickPollResponse>, Error>(
            result.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public async Task<Option<bool, Error>> Vote(VoteRequest request)
    {
        var pollData = await GetPollData(request.PollId);
        if (pollData == null || pollData.Status != PollStatusEnum.Active)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("QuickPoll.NotActive", "Poll is not active"));
        }

        var voteKey = $"{VOTE_KEY_PREFIX}{request.PollId}:{request.SessionParticipantId}";
        var existingVote = await _cacheService.Get<string>(voteKey);
        if (!string.IsNullOrEmpty(existingVote))
        {
            return Option.None<bool, Error>(
                Error.Conflict("QuickPoll.AlreadyVoted", "You have already voted in this poll"));
        }

        switch (pollData.PollType)
        {
            case PollTypeEnum.SingleChoice:
                var singleOptionId = request.OptionId ?? request.OptionIds.FirstOrDefault();
                if (singleOptionId == Guid.Empty || !pollData.Options.Any(o => o.OptionId == singleOptionId))
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("QuickPoll.InvalidOption", "A valid option is required"));
                }
                await _cacheService.Set(voteKey, singleOptionId.ToString(), PollPersistenceTtl);
                await IncrementVoteCount(request.PollId, singleOptionId);
                break;

            case PollTypeEnum.MultiSelect:
                var selections = BuildSelectionList(request);
                if (selections.Count == 0)
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("QuickPoll.InvalidSelection", "At least one option must be selected"));
                }
                if (pollData.MaxSelections.HasValue && selections.Count > pollData.MaxSelections.Value)
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("QuickPoll.TooManySelections", $"You can select up to {pollData.MaxSelections.Value} options"));
                }
                if (!selections.TrueForAll(id => pollData.Options.Any(o => o.OptionId == id)))
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("QuickPoll.InvalidOption", "One or more selected options are invalid"));
                }
                await _cacheService.Set(voteKey, string.Join(',', selections), PollPersistenceTtl);
                foreach (var optionId in selections)
                {
                    await IncrementVoteCount(request.PollId, optionId);
                }
                break;

            case PollTypeEnum.Rating:
                if (!request.RatingValue.HasValue)
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("QuickPoll.MissingRating", "Rating value is required"));
                }
                var scaleMin = pollData.RatingScaleMin ?? 1;
                var scaleMax = pollData.RatingScaleMax ?? 5;
                var rating = Math.Clamp(request.RatingValue.Value, scaleMin, scaleMax);
                await _cacheService.Set(voteKey, rating.ToString(), PollPersistenceTtl);
                var bucketValue = (int)Math.Round(rating, MidpointRounding.AwayFromZero);
                var bucketOption = pollData.Options.First(o => o.RatingValue == bucketValue);
                await IncrementVoteCount(request.PollId, bucketOption.OptionId);
                pollData.RatingCount++;
                pollData.RatingTotal += rating;
                await SavePollData(pollData);
                break;

            default:
                return Option.None<bool, Error>(
                    Error.ValidationError("QuickPoll.UnsupportedType", "Unsupported poll type"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> ClosePoll(Guid pollId)
    {
        var pollData = await GetPollData(pollId);
        if (pollData == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("QuickPoll.NotFound", "Poll not found"));
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null ||
            !await _sessionRepository.CheckUserIsHost(pollData.SessionId, currentUserId.Value))
        {
            return Option.None<bool, Error>(
                Error.Forbidden("QuickPoll.NotHost", "Only the host can close a poll"));
        }

        pollData.Status = PollStatusEnum.Closed;
        pollData.ClosedAt = DateTime.UtcNow;
        pollData.ExpiresAt = null;
        await SavePollData(pollData);

        var activePollKey = $"{ACTIVE_POLL_KEY_PREFIX}{pollData.SessionId}";
        var currentActive = await _cacheService.Get<string>(activePollKey);
        if (!string.IsNullOrEmpty(currentActive) && Guid.TryParse(currentActive, out var activeId) && activeId == pollId)
        {
            await _cacheService.Remove(activePollKey);
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<QuickPollResponse, Error>> ActivatePoll(Guid pollId)
    {
        var pollData = await GetPollData(pollId);
        if (pollData == null)
        {
            return Option.None<QuickPollResponse, Error>(
                Error.NotFound("QuickPoll.NotFound", "Poll not found"));
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null ||
            !await _sessionRepository.CheckUserIsHost(pollData.SessionId, currentUserId.Value))
        {
            return Option.None<QuickPollResponse, Error>(
                Error.Forbidden("QuickPoll.NotHost", "Only the host can activate a poll"));
        }

        if (pollData.Status == PollStatusEnum.Active)
        {
            return Option.Some<QuickPollResponse, Error>(await BuildPollResponse(pollData));
        }

        // Close current active poll if any
        var activePollKey = $"{ACTIVE_POLL_KEY_PREFIX}{pollData.SessionId}";
        var currentActive = await _cacheService.Get<string>(activePollKey);
        if (!string.IsNullOrEmpty(currentActive) && Guid.TryParse(currentActive, out var activeId) && activeId != pollId)
        {
            var activePollData = await GetPollData(activeId);
            if (activePollData != null)
            {
                activePollData.Status = PollStatusEnum.Closed;
                activePollData.ClosedAt = DateTime.UtcNow;
                await SavePollData(activePollData);
            }
        }

        pollData.Status = PollStatusEnum.Active;
        pollData.ActivatedAt = DateTime.UtcNow;
        pollData.ExpiresAt = pollData.ActivatedAt.Value.AddMinutes(pollData.DurationMinutes);
        await SavePollData(pollData);
        await SetActivePoll(pollData.SessionId, pollId);

        return Option.Some<QuickPollResponse, Error>(await BuildPollResponse(pollData));
    }

    #region Helpers

    private Error? ValidatePollRequest(CreateQuickPollRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return Error.ValidationError("QuickPoll.InvalidQuestion", "Question is required");
        }

        switch (request.PollType)
        {
            case PollTypeEnum.SingleChoice:
                if (request.Options == null || request.Options.Count < 2)
                {
                    return Error.ValidationError("QuickPoll.InvalidOptions", "Single choice polls require at least two options");
                }
                break;
            case PollTypeEnum.MultiSelect:
                if (request.Options == null || request.Options.Count < 2)
                {
                    return Error.ValidationError("QuickPoll.InvalidOptions", "Multi select polls require at least two options");
                }
                if (request.MaxSelections.HasValue && request.MaxSelections.Value <= 0)
                {
                    return Error.ValidationError("QuickPoll.InvalidMaxSelections", "Max selections must be greater than zero");
                }
                break;
            case PollTypeEnum.Rating:
                if (!request.RatingScaleMin.HasValue || !request.RatingScaleMax.HasValue)
                {
                    request.RatingScaleMin = 1;
                    request.RatingScaleMax = 5;
                }
                if (request.RatingScaleMin >= request.RatingScaleMax)
                {
                    return Error.ValidationError("QuickPoll.InvalidScale", "Rating scale min must be less than max");
                }
                break;
            default:
                return Error.ValidationError("QuickPoll.UnsupportedType", "Unsupported poll type");
        }

        return null;
    }

    private PollData BuildPollData(Guid pollId, CreateQuickPollRequest request, DateTime createdAt)
    {
        var pollData = new PollData
        {
            PollId = pollId,
            SessionId = request.SessionId,
            Question = request.Question.Trim(),
            PollType = request.PollType,
            AllowMultipleSelections = request.PollType == PollTypeEnum.MultiSelect,
            MaxSelections = request.MaxSelections,
            RatingScaleMin = request.RatingScaleMin,
            RatingScaleMax = request.RatingScaleMax,
            Status = request.AutoActivate ? PollStatusEnum.Active : PollStatusEnum.Draft,
            CreatedAt = createdAt,
            ActivatedAt = request.AutoActivate ? createdAt : null,
            ExpiresAt = request.AutoActivate ? createdAt.AddMinutes(request.DurationMinutes) : null,
            DurationMinutes = Math.Max(1, request.DurationMinutes)
        };

        pollData.Options = BuildPollOptions(request, pollData);
        return pollData;
    }

    private List<PollOption> BuildPollOptions(CreateQuickPollRequest request, PollData pollData)
    {
        if (request.PollType == PollTypeEnum.Rating)
        {
            var min = pollData.RatingScaleMin ?? 1;
            var max = pollData.RatingScaleMax ?? 5;
            return Enumerable.Range(min, max - min + 1)
                .Select((value, index) => new PollOption
                {
                    OptionId = Guid.NewGuid(),
                    Text = value.ToString(),
                    DisplayOrder = index,
                    RatingValue = value
                }).ToList();
        }

        return request.Options
            .Select((text, index) => new PollOption
            {
                OptionId = Guid.NewGuid(),
                Text = text.Trim(),
                DisplayOrder = index
            }).ToList();
    }

    private async Task SavePollData(PollData pollData)
    {
        var pollKey = $"{POLL_KEY_PREFIX}{pollData.PollId}";
        var json = JsonSerializer.Serialize(pollData, _serializerOptions);
        await _cacheService.Set(pollKey, json, PollPersistenceTtl);
    }

    private async Task<PollData?> GetPollData(Guid pollId)
    {
        var pollKey = $"{POLL_KEY_PREFIX}{pollId}";
        var pollJson = await _cacheService.Get<string>(pollKey);
        return string.IsNullOrEmpty(pollJson)
            ? null
            : JsonSerializer.Deserialize<PollData>(pollJson, _serializerOptions);
    }

    private async Task SetActivePoll(Guid sessionId, Guid pollId)
    {
        await _cacheService.Set($"{ACTIVE_POLL_KEY_PREFIX}{sessionId}", pollId.ToString(), PollPersistenceTtl);
    }

    private async Task AppendPollToHistory(Guid sessionId, Guid pollId)
    {
        var historyKey = $"{SESSION_POLLS_KEY_PREFIX}{sessionId}";
        var pollIds = await _cacheService.Get<List<Guid>>(historyKey) ?? new List<Guid>();
        pollIds.Remove(pollId);
        pollIds.Add(pollId);
        await _cacheService.Set(historyKey, pollIds, PollPersistenceTtl);
    }

    private async Task<QuickPollResponse> BuildPollResponse(PollData pollData)
    {
        var optionsWithVotes = new List<PollOptionResponse>();
        var totalVotes = 0;

        foreach (var option in pollData.Options.OrderBy(o => o.DisplayOrder))
        {
            var voteCount = await GetVoteCountForOption(pollData.PollId, option.OptionId);
            totalVotes += voteCount;
            optionsWithVotes.Add(new PollOptionResponse
            {
                OptionId = option.OptionId,
                Text = option.Text,
                VoteCount = voteCount,
                Percentage = 0
            });
        }

        foreach (var option in optionsWithVotes)
        {
            option.Percentage = totalVotes > 0
                ? Math.Round((decimal)option.VoteCount / totalVotes * 100, 2)
                : 0;
        }

        return new QuickPollResponse
        {
            PollId = pollData.PollId,
            SessionId = pollData.SessionId,
            Question = pollData.Question,
            PollType = pollData.PollType,
            Status = pollData.Status,
            AllowMultipleSelections = pollData.AllowMultipleSelections,
            MaxSelections = pollData.MaxSelections,
            Options = optionsWithVotes,
            TotalVotes = pollData.PollType == PollTypeEnum.Rating ? pollData.RatingCount : totalVotes,
            CreatedAt = pollData.CreatedAt,
            ActivatedAt = pollData.ActivatedAt,
            ExpiresAt = pollData.ExpiresAt,
            ClosedAt = pollData.ClosedAt,
            RatingSummary = pollData.PollType == PollTypeEnum.Rating
                ? new RatingSummaryResponse
                {
                    ScaleMin = pollData.RatingScaleMin,
                    ScaleMax = pollData.RatingScaleMax,
                    TotalVotes = pollData.RatingCount,
                    Average = pollData.RatingCount > 0
                        ? Math.Round(pollData.RatingTotal / pollData.RatingCount, 2)
                        : 0
                }
                : null
        };
    }

    private async Task<int> GetVoteCountForOption(Guid pollId, Guid optionId)
    {
        var voteCountKey = $"{VOTE_KEY_PREFIX}count:{pollId}:{optionId}";
        var countStr = await _cacheService.Get<string>(voteCountKey);
        return int.TryParse(countStr, out var count) ? count : 0;
    }

    private async Task IncrementVoteCount(Guid pollId, Guid optionId)
    {
        var voteCountKey = $"{VOTE_KEY_PREFIX}count:{pollId}:{optionId}";
        var currentCount = await GetVoteCountForOption(pollId, optionId);
        await _cacheService.Set(voteCountKey, (currentCount + 1).ToString(), PollPersistenceTtl);
    }

    private static List<Guid> BuildSelectionList(VoteRequest request)
    {
        var selections = new List<Guid>();
        if (request.OptionId.HasValue && request.OptionId.Value != Guid.Empty)
        {
            selections.Add(request.OptionId.Value);
        }

        if (request.OptionIds != null)
        {
            selections.AddRange(request.OptionIds.Where(id => id != Guid.Empty));
        }

        return selections.Distinct().ToList();
    }

    private class PollData
    {
        public Guid PollId { get; set; }
        public Guid SessionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOption> Options { get; set; } = new();
        public PollTypeEnum PollType { get; set; }
        public PollStatusEnum Status { get; set; }
        public bool AllowMultipleSelections { get; set; }
        public int? MaxSelections { get; set; }
        public int? RatingScaleMin { get; set; }
        public int? RatingScaleMax { get; set; }
        public int RatingCount { get; set; }
        public decimal RatingTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int DurationMinutes { get; set; }
    }

    private class PollOption
    {
        public Guid OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int? RatingValue { get; set; }
    }

    #endregion
}
