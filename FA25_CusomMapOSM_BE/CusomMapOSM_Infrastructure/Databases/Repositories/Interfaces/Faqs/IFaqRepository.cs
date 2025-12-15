using CusomMapOSM_Domain.Entities.Faqs;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;

public interface IFaqRepository
{
    Task<List<Faq>> GetAllFaqsAsync(CancellationToken ct = default);
    Task<List<Faq>> GetFaqsByCategoryAsync(string category, CancellationToken ct = default);
    Task<Faq?> GetFaqByIdAsync(int faqId, CancellationToken ct = default);
    Task<List<string>> GetFaqCategoriesAsync(CancellationToken ct = default);
}
