using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Faqs;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Faqs;

public interface IFaqService
{
    Task<Option<GetAllFaqsResponse, Error>> GetAllFaqsAsync(CancellationToken ct = default);
    Task<Option<GetFaqsByCategoryResponse, Error>> GetFaqsByCategoryAsync(string category, CancellationToken ct = default);
    Task<Option<GetFaqByIdResponse, Error>> GetFaqByIdAsync(int faqId, CancellationToken ct = default);
    Task<Option<GetFaqCategoriesResponse, Error>> GetFaqCategoriesAsync(CancellationToken ct = default);
}
