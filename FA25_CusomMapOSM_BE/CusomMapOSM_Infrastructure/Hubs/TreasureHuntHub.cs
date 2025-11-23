using CusomMapOSM_Application.Interfaces.Features.TreasureHunts;
using CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CusomMapOSM_Infrastructure.Hubs;

public class TreasureHuntHub : Hub
{
    private readonly ITreasureHuntService _treasureHuntService;

    public TreasureHuntHub(ITreasureHuntService treasureHuntService)
    {
        _treasureHuntService = treasureHuntService;
    }

    // Join a treasure hunt game room
    public async Task JoinGame(Guid treasureHuntId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"hunt_{treasureHuntId}");
    }

    // Leave a treasure hunt game room
    public async Task LeaveGame(Guid treasureHuntId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"hunt_{treasureHuntId}");
    }

    // Teacher creates a treasure hunt
    public async Task CreateTreasureHunt(CreateTreasureHuntRequest request)
    {
        var result = await _treasureHuntService.CreateTreasureHunt(request);
        
        await result.Match(
            async hunt =>
            {
                // Broadcast new game to all session participants
                await Clients.Group($"session_{request.SessionId}")
                    .SendAsync("TreasureHuntCreated", hunt);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Student submits a guess
    public async Task SubmitGuess(SubmitGuessRequest request)
    {
        var result = await _treasureHuntService.SubmitGuess(request);
        
        await result.Match(
            async guessResult =>
            {
                // Send result to the student
                await Clients.Caller.SendAsync("GuessResult", guessResult);

                // Get updated leaderboard
                var leaderboardOption = await _treasureHuntService.GetLeaderboard(request.TreasureHuntId);
                await leaderboardOption.Match(
                    async leaderboard =>
                    {
                        // Broadcast updated leaderboard to all participants
                        await Clients.Group($"hunt_{request.TreasureHuntId}")
                            .SendAsync("LeaderboardUpdated", leaderboard);
                    },
                    async _ => { }
                );
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Get current leaderboard (for late joiners)
    public async Task GetLeaderboard(Guid treasureHuntId)
    {
        var result = await _treasureHuntService.GetLeaderboard(treasureHuntId);
        
        await result.Match(
            async leaderboard =>
            {
                await Clients.Caller.SendAsync("Leaderboard", leaderboard);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Teacher ends the game
    public async Task EndTreasureHunt(Guid treasureHuntId)
    {
        var result = await _treasureHuntService.EndTreasureHunt(treasureHuntId);
        
        await result.Match(
            async success =>
            {
                // Get final leaderboard
                var leaderboardOption = await _treasureHuntService.GetLeaderboard(treasureHuntId);
                await leaderboardOption.Match(
                    async leaderboard =>
                    {
                        // Broadcast game ended with final results
                        await Clients.Group($"hunt_{treasureHuntId}")
                            .SendAsync("TreasureHuntEnded", new { treasureHuntId, leaderboard });
                    },
                    async _ =>
                    {
                        await Clients.Group($"hunt_{treasureHuntId}")
                            .SendAsync("TreasureHuntEnded", new { treasureHuntId });
                    }
                );
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }
}
