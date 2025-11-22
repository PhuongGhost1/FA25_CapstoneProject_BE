using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Request;
using CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.TreasureHunts;

public interface ITreasureHuntService
{
    Task<Option<TreasureHuntResponse, Error>> CreateTreasureHunt(CreateTreasureHuntRequest request);
    Task<Option<TreasureHuntResponse, Error>> GetTreasureHunt(Guid treasureHuntId);
    Task<Option<TreasureHuntResponse, Error>> GetActiveTreasureHunt(Guid sessionId);
    Task<Option<SubmitGuessResponse, Error>> SubmitGuess(SubmitGuessRequest request);
    Task<Option<LeaderboardResponse, Error>> GetLeaderboard(Guid treasureHuntId);
    Task<Option<bool, Error>> EndTreasureHunt(Guid treasureHuntId);
}
