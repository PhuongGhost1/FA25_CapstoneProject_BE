using CusomMapOSM_Domain.Entities.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Faqs;

public class FaqRepository : IFaqRepository
{
    private readonly CustomMapOSMDbContext _context;

    public FaqRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<List<Faq>> GetAllFaqsAsync(CancellationToken ct = default)
    {
        return await _context.Faqs
            .OrderBy(f => f.Category)
            .ThenBy(f => f.FaqId)
            .ToListAsync(ct);
    }

    public async Task<List<Faq>> GetFaqsByCategoryAsync(string category, CancellationToken ct = default)
    {
        return await _context.Faqs
            .Where(f => f.Category == category)
            .OrderBy(f => f.FaqId)
            .ToListAsync(ct);
    }

    public async Task<Faq?> GetFaqByIdAsync(int faqId, CancellationToken ct = default)
    {
        return await _context.Faqs
            .FirstOrDefaultAsync(f => f.FaqId == faqId, ct);
    }

    public async Task<List<string>> GetFaqCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Faqs
            .Select(f => f.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }
}
