using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.QuickPolls;

public interface IQuickPollService
{
    Task<Option<QuickPollResponse, Error>> CreatePoll(CreateQuickPollRequest request);
    Task<Option<QuickPollResponse, Error>> GetActivePoll(Guid sessionId);
    Task<Option<QuickPollResponse, Error>> GetPollResults(Guid pollId);
    Task<Option<bool, Error>> Vote(VoteRequest request);
    Task<Option<bool, Error>> ClosePoll(Guid pollId);
    Task<Option<QuickPollResponse, Error>> ActivatePoll(Guid pollId);
    Task<Option<IReadOnlyList<QuickPollResponse>, Error>> GetPollHistory(Guid sessionId);
}
