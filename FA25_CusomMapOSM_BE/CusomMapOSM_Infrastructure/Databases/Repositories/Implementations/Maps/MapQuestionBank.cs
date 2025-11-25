using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapQuestionBankRepository : IMapQuestionBankRepository
{
    private readonly CustomMapOSMDbContext _context;
    public MapQuestionBankRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }
    public async Task<bool> AddQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default)
    {
        await _context.MapQuestionBanks.AddAsync(mapQuestionBank, ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default)
    {
        _context.MapQuestionBanks.Remove(mapQuestionBank);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default)
    {
        _context.MapQuestionBanks.Update(mapQuestionBank);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<MapQuestionBank?> GetQuestionBank(Guid mapId, CancellationToken ct = default)
    {
        return await _context.MapQuestionBanks.FirstOrDefaultAsync(x => x.MapId == mapId, ct);
    }

    public async Task<List<MapQuestionBank>> GetQuestionBanks(Guid mapId, CancellationToken ct = default)
    {
        return await _context.MapQuestionBanks.Where(x => x.MapId == mapId).ToListAsync(ct);
    }
}