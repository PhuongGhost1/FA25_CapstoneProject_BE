using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Response;
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

    // Question CRUD
    Task<Option<Guid, Error>> CreateQuestion(CreateQuestionRequest request);
    Task<Option<List<QuestionDTO>, Error>> GetQuestionsByQuestionBankId(Guid questionBankId);
    Task<Option<Guid, Error>> UpdateQuestion(UpdateQuestionRequest request);
    Task<Option<bool, Error>> DeleteQuestion(Guid questionId);

    // Map Question Bank
    Task<Option<bool, Error>> AttachQuestionBankToMap(Guid questionBankId, AttachQuestionBankToMapRequest request);
    Task<Option<bool, Error>> DetachQuestionBankFromMap(Guid mapId);
    Task<Option<List<QuestionDTO>, Error>>  GetQuestionBanksByMapId(Guid mapId);
}