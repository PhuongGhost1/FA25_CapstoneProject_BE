using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.QuestionBanks;

public interface IQuestionBankService
{
    // Question Bank CRUD
    Task<Option<QuestionBankDTO, Error>> CreateQuestionBank(CreateQuestionBankRequest request);
    Task<Option<QuestionBankDTO, Error>> GetQuestionBankById(Guid questionBankId);
    Task<Option<List<QuestionBankDTO>, Error>> GetMyQuestionBanks();
    Task<Option<List<QuestionBankDTO>, Error>> GetPublicQuestionBanks();
    Task<Option<QuestionBankDTO, Error>> UpdateQuestionBank(Guid questionBankId, UpdateQuestionBankRequest request);
    Task<Option<bool, Error>> DeleteQuestionBank(Guid questionBankId);
    Task<Option<QuestionBankDTO, Error>> AddQuestionBankTags(Guid questionBankId, UpdateQuestionBankTagsRequest request);
    Task<Option<QuestionBankDTO, Error>> ReplaceQuestionBankTags(Guid questionBankId, UpdateQuestionBankTagsRequest request);

    // Organization-based Question Banks
    Task<Option<List<QuestionBankDTO>, Error>> GetMyQuestionBanksByOrganization(Guid orgId);
    Task<Option<List<QuestionBankDTO>, Error>> GetPublicQuestionBanksByOrganization(Guid orgId);
    Task<Option<QuestionBankDTO, Error>> DuplicateQuestionBank(Guid questionBankId, Guid targetWorkspaceId);

    // Question CRUD
    Task<Option<Guid, Error>> CreateQuestion(CreateQuestionRequest request);
    Task<Option<List<QuestionDTO>, Error>> GetQuestionsByQuestionBankId(Guid questionBankId);
    Task<Option<Guid, Error>> UpdateQuestion(UpdateQuestionRequest request);
    Task<Option<bool, Error>> DeleteQuestion(Guid questionId);

    // Session Question Bank
    Task<Option<bool, Error>> AttachQuestionBankToSession(Guid questionBankId, AttachQuestionBankToSessionRequest request);
    Task<Option<bool, Error>> DetachQuestionBankFromSession(Guid sessionId);
    Task<Option<List<QuestionDTO>, Error>> GetQuestionBanksBySessionId(Guid sessionId);
    Task<Option<List<SessionQuestionBankResponse>, Error>> GetSessionsByQuestionBankId(Guid questionBankId);

    // File Uploads
    Task<Option<string, Error>> UploadQuestionImage(IFormFile file, Guid? questionBankId);
    Task<Option<string, Error>> UploadQuestionAudio(IFormFile file, Guid? questionBankId);
    Task<Option<string, Error>> UploadOptionImage(IFormFile file, Guid? questionBankId);
}