using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;

public class StudentResponseRepository : IStudentResponseRepository
{
    private readonly CustomMapOSMDbContext _context;

    public StudentResponseRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateResponse(StudentResponse response)
    {
        _context.StudentResponses.Add(response);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<StudentResponse?> GetResponseById(Guid responseId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionQuestion)
                .ThenInclude(sq => sq!.Question)
            .Include(sr => sr.SessionParticipant)
            .Include(sr => sr.QuestionOption)
            .FirstOrDefaultAsync(sr => sr.StudentResponseId == responseId);
    }

    public async Task<List<StudentResponse>> GetResponsesBySessionQuestion(Guid sessionQuestionId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionParticipant)
                .ThenInclude(sp => sp!.User)
            .Include(sr => sr.QuestionOption)
            .Where(sr => sr.SessionQuestionId == sessionQuestionId)
            .OrderBy(sr => sr.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<StudentResponse>> GetResponsesByParticipant(Guid participantId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionQuestion)
                .ThenInclude(sq => sq!.Question)
            .Include(sr => sr.QuestionOption)
            .Where(sr => sr.SessionParticipantId == participantId)
            .OrderBy(sr => sr.SubmittedAt)
            .ToListAsync();
    }

    public async Task<StudentResponse?> GetParticipantResponseForQuestion(Guid sessionQuestionId, Guid participantId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionQuestion)
                .ThenInclude(sq => sq!.Question)
            .Include(sr => sr.QuestionOption)
            .FirstOrDefaultAsync(sr => sr.SessionQuestionId == sessionQuestionId && sr.SessionParticipantId == participantId);
    }

    public async Task<bool> CheckResponseExists(Guid responseId)
    {
        return await _context.StudentResponses.AnyAsync(sr => sr.StudentResponseId == responseId);
    }

    public async Task<bool> CheckParticipantAlreadyAnswered(Guid sessionQuestionId, Guid participantId)
    {
        return await _context.StudentResponses
            .AnyAsync(sr => sr.SessionQuestionId == sessionQuestionId && sr.SessionParticipantId == participantId);
    }

    public async Task<int> GetCorrectResponseCount(Guid sessionQuestionId)
    {
        return await _context.StudentResponses
            .CountAsync(sr => sr.SessionQuestionId == sessionQuestionId && sr.IsCorrect);
    }

    public async Task<int> GetTotalResponseCount(Guid sessionQuestionId)
    {
        return await _context.StudentResponses
            .CountAsync(sr => sr.SessionQuestionId == sessionQuestionId);
    }

    public async Task<decimal> GetAverageResponseTime(Guid sessionQuestionId)
    {
        var responses = await _context.StudentResponses
            .Where(sr => sr.SessionQuestionId == sessionQuestionId)
            .ToListAsync();

        return responses.Any() ? responses.Average(r => r.ResponseTimeSeconds) : 0;
    }

    public async Task<List<StudentResponse>> GetResponsesForAnalytics(Guid sessionQuestionId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionParticipant)
            .Include(sr => sr.QuestionOption)
            .Where(sr => sr.SessionQuestionId == sessionQuestionId)
            .OrderBy(sr => sr.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<StudentResponse>> GetMapPinResponses(Guid sessionQuestionId)
    {
        return await _context.StudentResponses
            .Include(sr => sr.SessionParticipant)
                .ThenInclude(sp => sp!.User)
            .Where(sr => sr.SessionQuestionId == sessionQuestionId &&
                        sr.ResponseLatitude.HasValue &&
                        sr.ResponseLongitude.HasValue)
            .OrderBy(sr => sr.DistanceErrorMeters)
            .ToListAsync();
    }
}