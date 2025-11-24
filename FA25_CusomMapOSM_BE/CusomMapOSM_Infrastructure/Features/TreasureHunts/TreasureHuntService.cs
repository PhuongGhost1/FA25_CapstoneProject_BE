using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.TreasureHunts;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Request;
using CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Response;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Optional;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.TreasureHunts;

public class TreasureHuntService : ITreasureHuntService
{
    private readonly IRedisCacheService _cacheService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionParticipantRepository _participantRepository;
    private readonly ICurrentUserService _currentUserService;
    private const string HUNT_KEY_PREFIX = "treasurehunt:";
    private const string ACTIVE_HUNT_KEY_PREFIX = "treasurehunt:active:";
    private const string SUBMISSION_KEY_PREFIX = "treasurehunt:submission:";
    private const string LEADERBOARD_KEY_PREFIX = "treasurehunt:leaderboard:";

    public TreasureHuntService(
        IRedisCacheService cacheService,
        ISessionRepository sessionRepository,
        ISessionParticipantRepository participantRepository,
        ICurrentUserService currentUserService)
    {
        _cacheService = cacheService;
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<TreasureHuntResponse, Error>> CreateTreasureHunt(CreateTreasureHuntRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.Unauthorized("TreasureHunt.Unauthorized", "User not authenticated"));
        }

        // Verify user is session host
        var isHost = await _sessionRepository.CheckUserIsHost(request.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.Forbidden("TreasureHunt.NotHost", "Only session host can create treasure hunts"));
        }

        // Validate clues
        if (request.Clues == null || !request.Clues.Any())
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.ValidationError("TreasureHunt.NoClues", "At least one clue is required"));
        }

        var huntId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddMinutes(request.DurationMinutes);

        var clues = request.Clues.Select((clue, index) => new
        {
            clueId = Guid.NewGuid(),
            clueText = clue.ClueText,
            targetLatitude = clue.TargetLatitude,
            targetLongitude = clue.TargetLongitude,
            points = clue.Points,
            displayOrder = index
        }).ToList();

        var huntData = new
        {
            treasureHuntId = huntId,
            sessionId = request.SessionId,
            title = request.Title,
            clues = clues,
            acceptanceRadiusMeters = request.AcceptanceRadiusMeters,
            isActive = true,
            createdAt = createdAt,
            expiresAt = expiresAt
        };

        var huntKey = $"{HUNT_KEY_PREFIX}{huntId}";
        var activeHuntKey = $"{ACTIVE_HUNT_KEY_PREFIX}{request.SessionId}";
        
        var huntJson = JsonSerializer.Serialize(huntData);
        await _cacheService.Set(huntKey, huntJson, TimeSpan.FromMinutes(request.DurationMinutes + 10));
        await _cacheService.Set(activeHuntKey, huntId.ToString(), TimeSpan.FromMinutes(request.DurationMinutes + 10));

        return Option.Some<TreasureHuntResponse, Error>(new TreasureHuntResponse
        {
            TreasureHuntId = huntId,
            SessionId = request.SessionId,
            Title = request.Title,
            Clues = clues.Select(c => new TreasureClueResponse
            {
                ClueId = c.clueId,
                ClueText = c.clueText,
                Points = c.points,
                DisplayOrder = c.displayOrder
            }).ToList(),
            AcceptanceRadiusMeters = request.AcceptanceRadiusMeters,
            IsActive = true,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        });
    }

    public async Task<Option<TreasureHuntResponse, Error>> GetTreasureHunt(Guid treasureHuntId)
    {
        var huntKey = $"{HUNT_KEY_PREFIX}{treasureHuntId}";
        var huntJson = await _cacheService.Get<string>(huntKey);
        
        if (string.IsNullOrEmpty(huntJson))
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.NotFound("TreasureHunt.NotFound", "Treasure hunt not found or expired"));
        }

        var huntData = JsonSerializer.Deserialize<TreasureHuntData>(huntJson);
        if (huntData == null)
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.Failure("TreasureHunt.InvalidData", "Invalid treasure hunt data"));
        }

        return Option.Some<TreasureHuntResponse, Error>(new TreasureHuntResponse
        {
            TreasureHuntId = huntData.TreasureHuntId,
            SessionId = huntData.SessionId,
            Title = huntData.Title,
            Clues = huntData.Clues.Select(c => new TreasureClueResponse
            {
                ClueId = c.ClueId,
                ClueText = c.ClueText,
                Points = c.Points,
                DisplayOrder = c.DisplayOrder
            }).ToList(),
            AcceptanceRadiusMeters = huntData.AcceptanceRadiusMeters,
            IsActive = huntData.IsActive,
            CreatedAt = huntData.CreatedAt,
            ExpiresAt = huntData.ExpiresAt
        });
    }

    public async Task<Option<TreasureHuntResponse, Error>> GetActiveTreasureHunt(Guid sessionId)
    {
        var activeHuntKey = $"{ACTIVE_HUNT_KEY_PREFIX}{sessionId}";
        var huntIdStr = await _cacheService.Get<string>(activeHuntKey);
        
        if (string.IsNullOrEmpty(huntIdStr) || !Guid.TryParse(huntIdStr, out var huntId))
        {
            return Option.None<TreasureHuntResponse, Error>(
                Error.NotFound("TreasureHunt.NoActiveHunt", "No active treasure hunt for this session"));
        }

        return await GetTreasureHunt(huntId);
    }

    public async Task<Option<SubmitGuessResponse, Error>> SubmitGuess(SubmitGuessRequest request)
    {
        // Get hunt data
        var huntKey = $"{HUNT_KEY_PREFIX}{request.TreasureHuntId}";
        var huntJson = await _cacheService.Get<string>(huntKey);
        
        if (string.IsNullOrEmpty(huntJson))
        {
            return Option.None<SubmitGuessResponse, Error>(
                Error.NotFound("TreasureHunt.NotFound", "Treasure hunt not found or expired"));
        }

        var huntData = JsonSerializer.Deserialize<TreasureHuntData>(huntJson);
        if (huntData == null || !huntData.IsActive)
        {
            return Option.None<SubmitGuessResponse, Error>(
                Error.ValidationError("TreasureHunt.NotActive", "Treasure hunt is not active"));
        }

        // Find the clue
        var clue = huntData.Clues.FirstOrDefault(c => c.ClueId == request.ClueId);
        if (clue == null)
        {
            return Option.None<SubmitGuessResponse, Error>(
                Error.NotFound("TreasureHunt.ClueNotFound", "Clue not found"));
        }

        // Check if already submitted for this clue
        var submissionKey = $"{SUBMISSION_KEY_PREFIX}{request.TreasureHuntId}:{request.ClueId}:{request.SessionParticipantId}";
        var existingSubmission = await _cacheService.Get<string>(submissionKey);
        
        if (!string.IsNullOrEmpty(existingSubmission))
        {
            return Option.None<SubmitGuessResponse, Error>(
                Error.Conflict("TreasureHunt.AlreadySubmitted", "You have already submitted a guess for this clue"));
        }

        // Calculate distance using Haversine formula
        var distance = CalculateDistance(
            (double)clue.TargetLatitude,
            (double)clue.TargetLongitude,
            (double)request.GuessLatitude,
            (double)request.GuessLongitude);

        var isCorrect = distance <= huntData.AcceptanceRadiusMeters;
        var pointsEarned = isCorrect ? clue.Points : 0;

        // Record submission
        var submission = new
        {
            clueId = request.ClueId,
            guessLatitude = request.GuessLatitude,
            guessLongitude = request.GuessLongitude,
            distance = distance,
            isCorrect = isCorrect,
            pointsEarned = pointsEarned,
            submittedAt = DateTime.UtcNow
        };

        var expiresAt = huntData.ExpiresAt ?? DateTime.UtcNow.AddHours(24);
        var ttl = expiresAt - DateTime.UtcNow;
        
        await _cacheService.Set(submissionKey, JsonSerializer.Serialize(submission), ttl);

        // Update leaderboard
        await UpdateLeaderboard(request.TreasureHuntId, request.SessionParticipantId, pointsEarned, ttl);

        var message = isCorrect 
            ? $"Correct! You found the treasure and earned {pointsEarned} points!" 
            : $"Not quite! You were {Math.Round(distance, 2)}m away. Try again!";

        return Option.Some<SubmitGuessResponse, Error>(new SubmitGuessResponse
        {
            IsCorrect = isCorrect,
            DistanceMeters = (decimal)distance,
            PointsEarned = pointsEarned,
            Message = message
        });
    }

    public async Task<Option<LeaderboardResponse, Error>> GetLeaderboard(Guid treasureHuntId)
    {
        var huntKey = $"{HUNT_KEY_PREFIX}{treasureHuntId}";
        var huntJson = await _cacheService.Get<string>(huntKey);
        
        if (string.IsNullOrEmpty(huntJson))
        {
            return Option.None<LeaderboardResponse, Error>(
                Error.NotFound("TreasureHunt.NotFound", "Treasure hunt not found"));
        }

        var huntData = JsonSerializer.Deserialize<TreasureHuntData>(huntJson);
        if (huntData == null)
        {
            return Option.None<LeaderboardResponse, Error>(
                Error.Failure("TreasureHunt.InvalidData", "Invalid treasure hunt data"));
        }

        var leaderboardKey = $"{LEADERBOARD_KEY_PREFIX}{treasureHuntId}";
        var leaderboardJson = await _cacheService.Get<string>(leaderboardKey);
        
        Dictionary<Guid, ParticipantScore> scores;
        if (string.IsNullOrEmpty(leaderboardJson))
        {
            scores = new Dictionary<Guid, ParticipantScore>();
        }
        else
        {
            scores = JsonSerializer.Deserialize<Dictionary<Guid, ParticipantScore>>(leaderboardJson) 
                ?? new Dictionary<Guid, ParticipantScore>();
        }

        var entries = scores
            .OrderByDescending(kvp => kvp.Value.TotalPoints)
            .ThenBy(kvp => kvp.Value.LastSubmissionTime)
            .Select((kvp, index) => new LeaderboardEntry
            {
                SessionParticipantId = kvp.Key,
                ParticipantName = kvp.Value.ParticipantName,
                TotalPoints = kvp.Value.TotalPoints,
                CluesFound = kvp.Value.CluesFound,
                Rank = index + 1
            })
            .ToList();

        return Option.Some<LeaderboardResponse, Error>(new LeaderboardResponse
        {
            TreasureHuntId = treasureHuntId,
            Title = huntData.Title,
            Entries = entries
        });
    }

    public async Task<Option<bool, Error>> EndTreasureHunt(Guid treasureHuntId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("TreasureHunt.Unauthorized", "User not authenticated"));
        }

        var huntKey = $"{HUNT_KEY_PREFIX}{treasureHuntId}";
        var huntJson = await _cacheService.Get<string>(huntKey);
        
        if (string.IsNullOrEmpty(huntJson))
        {
            return Option.None<bool, Error>(
                Error.NotFound("TreasureHunt.NotFound", "Treasure hunt not found"));
        }

        var huntData = JsonSerializer.Deserialize<TreasureHuntData>(huntJson);
        if (huntData == null)
        {
            return Option.None<bool, Error>(
                Error.Failure("TreasureHunt.InvalidData", "Invalid treasure hunt data"));
        }

        // Verify user is session host
        var isHost = await _sessionRepository.CheckUserIsHost(huntData.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("TreasureHunt.NotHost", "Only session host can end treasure hunts"));
        }

        // Mark as inactive
        huntData.IsActive = false;
        var updatedJson = JsonSerializer.Serialize(huntData);
        await _cacheService.Set(huntKey, updatedJson, TimeSpan.FromHours(1));

        // Remove from active hunts
        var activeHuntKey = $"{ACTIVE_HUNT_KEY_PREFIX}{huntData.SessionId}";
        await _cacheService.Remove(activeHuntKey);

        return Option.Some<bool, Error>(true);
    }

    private async Task UpdateLeaderboard(Guid treasureHuntId, Guid participantId, int pointsEarned, TimeSpan ttl)
    {
        var leaderboardKey = $"{LEADERBOARD_KEY_PREFIX}{treasureHuntId}";
        var leaderboardJson = await _cacheService.Get<string>(leaderboardKey);
        
        Dictionary<Guid, ParticipantScore> scores;
        if (string.IsNullOrEmpty(leaderboardJson))
        {
            scores = new Dictionary<Guid, ParticipantScore>();
        }
        else
        {
            scores = JsonSerializer.Deserialize<Dictionary<Guid, ParticipantScore>>(leaderboardJson) 
                ?? new Dictionary<Guid, ParticipantScore>();
        }

        if (scores.ContainsKey(participantId))
        {
            scores[participantId].TotalPoints += pointsEarned;
            if (pointsEarned > 0)
            {
                scores[participantId].CluesFound++;
            }
            scores[participantId].LastSubmissionTime = DateTime.UtcNow;
        }
        else
        {
            var participant = await _participantRepository.GetParticipantById(participantId);
            scores[participantId] = new ParticipantScore
            {
                ParticipantName = participant?.DisplayName ?? "Unknown",
                TotalPoints = pointsEarned,
                CluesFound = pointsEarned > 0 ? 1 : 0,
                LastSubmissionTime = DateTime.UtcNow
            };
        }

        var updatedJson = JsonSerializer.Serialize(scores);
        await _cacheService.Set(leaderboardKey, updatedJson, ttl);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth's radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // Helper classes for deserialization
    private class TreasureHuntData
    {
        public Guid TreasureHuntId { get; set; }
        public Guid SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<TreasureClue> Clues { get; set; } = new();
        public int AcceptanceRadiusMeters { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    private class TreasureClue
    {
        public Guid ClueId { get; set; }
        public string ClueText { get; set; } = string.Empty;
        public decimal TargetLatitude { get; set; }
        public decimal TargetLongitude { get; set; }
        public int Points { get; set; }
        public int DisplayOrder { get; set; }
    }

    private class ParticipantScore
    {
        public string ParticipantName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public int CluesFound { get; set; }
        public DateTime LastSubmissionTime { get; set; }
    }
}
