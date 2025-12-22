using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

/// <summary>
/// Comprehensive session summary for teachers to review after session ends
/// </summary>
public class SessionSummaryResponse
{
    // Session Overview
    public Guid SessionId { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    
    // Map Info
    public Guid MapId { get; set; }
    public string MapName { get; set; } = string.Empty;
    
    // Question Banks
    public List<QuestionBankSummary> QuestionBanks { get; set; } = new();
    
    // Overall Statistics
    public OverallStatistics Statistics { get; set; } = new();
    
    // Participant Analysis
    public ParticipantAnalysis ParticipantAnalysis { get; set; } = new();
    
    // Question-by-Question Breakdown
    public List<QuestionBreakdown> QuestionBreakdowns { get; set; } = new();
    
    // Top Performers
    public List<TopPerformer> TopPerformers { get; set; } = new();
    
    // Score Distribution (for histogram chart)
    public List<ScoreDistributionBucket> ScoreDistribution { get; set; } = new();
}

public class QuestionBankSummary
{
    public Guid QuestionBankId { get; set; }
    public string QuestionBankName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
}

public class OverallStatistics
{
    public int TotalParticipants { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalResponses { get; set; }
    
    // Averages
    public decimal AverageScore { get; set; }
    public decimal AverageAccuracyPercent { get; set; }
    public decimal AverageResponseTimeSeconds { get; set; }
    
    // Score range
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }
    public decimal MedianScore { get; set; }
    
    // Participation
    public decimal ParticipationRatePercent { get; set; }
    public int ParticipantsWithPerfectScore { get; set; }
    public int ParticipantsWithZeroScore { get; set; }
    
    // Question completion
    public decimal CompletionRatePercent { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int TotalIncorrectAnswers { get; set; }
}

public class ParticipantAnalysis
{
    public int TotalJoined { get; set; }
    public int ActiveThroughout { get; set; }
    public int LeftEarly { get; set; }
    public int GuestParticipants { get; set; }
    public int RegisteredUsers { get; set; }
    public decimal AverageQuestionsAnswered { get; set; }
}

public class QuestionBreakdown
{
    public Guid SessionQuestionId { get; set; }
    public Guid QuestionId { get; set; }
    public int QueueOrder { get; set; }
    public QuestionTypeEnum QuestionType { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public int Points { get; set; }
    public int TimeLimit { get; set; }
    
    // Response Stats
    public int TotalResponses { get; set; }
    public int CorrectResponses { get; set; }
    public int IncorrectResponses { get; set; }
    public decimal CorrectPercentage { get; set; }
    public decimal AverageResponseTimeSeconds { get; set; }
    public decimal AveragePointsEarned { get; set; }
    
    // For Multiple Choice Questions
    public List<OptionAnalysis>? OptionAnalysis { get; set; }
    
    // For Pin on Map Questions
    public decimal? AverageDistanceErrorMeters { get; set; }
    
    // For Text/Word Cloud Questions
    public List<WordFrequency>? CommonAnswers { get; set; }
    
    // Difficulty indicator
    public string DifficultyLevel { get; set; } = string.Empty; // Easy, Medium, Hard based on correct %
}

public class OptionAnalysis
{
    public Guid OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SelectCount { get; set; }
    public decimal SelectPercentage { get; set; }
}

public class WordFrequency
{
    public string Word { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class TopPerformer
{
    public int Rank { get; set; }
    public Guid SessionParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalAnswered { get; set; }
    public decimal AccuracyPercent { get; set; }
    public decimal AverageResponseTimeSeconds { get; set; }
}

public class ScoreDistributionBucket
{
    public string Label { get; set; } = string.Empty; // e.g., "0-100", "101-200"
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
