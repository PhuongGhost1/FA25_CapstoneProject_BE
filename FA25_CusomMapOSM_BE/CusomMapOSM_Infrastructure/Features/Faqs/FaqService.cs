using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Faqs;
using CusomMapOSM_Application.Models.DTOs.Features.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Faqs;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;

    public FaqService(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task<Option<GetAllFaqsResponse, Error>> GetAllFaqsAsync(CancellationToken ct = default)
    {
        try
        {
            var faqs = await _faqRepository.GetAllFaqsAsync(ct);

            var faqDtos = faqs.Select(f => new FaqDto
            {
                FaqId = f.FaqId,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                CreatedAt = f.CreatedAt
            }).ToList();

            return Option.Some<GetAllFaqsResponse, Error>(new GetAllFaqsResponse
            {
                Faqs = faqDtos
            });
        }
        catch (Exception ex)
        {
            return Option.None<GetAllFaqsResponse, Error>(Error.Failure("Faq.GetAll", $"Failed to retrieve FAQs: {ex.Message}"));
        }
    }

    public async Task<Option<GetFaqsByCategoryResponse, Error>> GetFaqsByCategoryAsync(string category, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return Option.None<GetFaqsByCategoryResponse, Error>(Error.ValidationError("Faq.Category", "Category cannot be empty"));
            }

            var faqs = await _faqRepository.GetFaqsByCategoryAsync(category, ct);

            var faqDtos = faqs.Select(f => new FaqDto
            {
                FaqId = f.FaqId,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                CreatedAt = f.CreatedAt
            }).ToList();

            return Option.Some<GetFaqsByCategoryResponse, Error>(new GetFaqsByCategoryResponse
            {
                Category = category,
                Faqs = faqDtos
            });
        }
        catch (Exception ex)
        {
            return Option.None<GetFaqsByCategoryResponse, Error>(Error.Failure("Faq.GetByCategory", $"Failed to retrieve FAQs for category '{category}': {ex.Message}"));
        }
    }

    public async Task<Option<GetFaqByIdResponse, Error>> GetFaqByIdAsync(int faqId, CancellationToken ct = default)
    {
        try
        {
            if (faqId <= 0)
            {
                return Option.None<GetFaqByIdResponse, Error>(Error.ValidationError("Faq.Id", "FAQ ID must be greater than 0"));
            }

            var faq = await _faqRepository.GetFaqByIdAsync(faqId, ct);

            if (faq == null)
            {
                return Option.None<GetFaqByIdResponse, Error>(Error.NotFound("Faq.NotFound", $"FAQ with ID {faqId} not found"));
            }

            var faqDto = new FaqDto
            {
                FaqId = faq.FaqId,
                Question = faq.Question,
                Answer = faq.Answer,
                Category = faq.Category,
                CreatedAt = faq.CreatedAt
            };

            return Option.Some<GetFaqByIdResponse, Error>(new GetFaqByIdResponse
            {
                Faq = faqDto
            });
        }
        catch (Exception ex)
        {
            return Option.None<GetFaqByIdResponse, Error>(Error.Failure("Faq.GetById", $"Failed to retrieve FAQ with ID {faqId}: {ex.Message}"));
        }
    }

    public async Task<Option<GetFaqCategoriesResponse, Error>> GetFaqCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var categories = await _faqRepository.GetFaqCategoriesAsync(ct);

            return Option.Some<GetFaqCategoriesResponse, Error>(new GetFaqCategoriesResponse
            {
                Categories = categories
            });
        }
        catch (Exception ex)
        {
            return Option.None<GetFaqCategoriesResponse, Error>(Error.Failure("Faq.GetCategories", $"Failed to retrieve FAQ categories: {ex.Message}"));
        }
    }
}
