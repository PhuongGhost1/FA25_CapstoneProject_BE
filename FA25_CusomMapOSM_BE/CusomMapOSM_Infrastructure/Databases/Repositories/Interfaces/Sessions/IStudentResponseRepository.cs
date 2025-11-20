using CusomMapOSM_Domain.Entities.Sessions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;

public interface IStudentResponseRepository
{
    // CRUD Operations
    Task<bool> CreateResponse(StudentResponse response);
    Task<StudentResponse?> GetResponseById(Guid responseId);
    Task<List<StudentResponse>> GetResponsesBySessionQuestion(Guid sessionQuestionId);
    Task<List<StudentResponse>> GetResponsesByParticipant(Guid participantId);
    Task<StudentResponse?> GetParticipantResponseForQuestion(Guid sessionQuestionId, Guid participantId);

    // Validation
    Task<bool> CheckResponseExists(Guid responseId);
    Task<bool> CheckParticipantAlreadyAnswered(Guid sessionQuestionId, Guid participantId);

    // Analytics
    Task<int> GetCorrectResponseCount(Guid sessionQuestionId);
    Task<int> GetTotalResponseCount(Guid sessionQuestionId);
    Task<decimal> GetAverageResponseTime(Guid sessionQuestionId);
    Task<List<StudentResponse>> GetResponsesForAnalytics(Guid sessionQuestionId);

    // Word Cloud Data (for WORD_CLOUD question type)
    Task<Dictionary<string, int>> GetWordCloudData(Guid sessionQuestionId);

    // Map Pin Data (for PIN_ON_MAP question type)
    Task<List<StudentResponse>> GetMapPinResponses(Guid sessionQuestionId);
}