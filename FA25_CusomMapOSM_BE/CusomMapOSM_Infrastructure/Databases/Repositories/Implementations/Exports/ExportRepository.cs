using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Exports;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Exports;

public class ExportRepository : IExportRepository
{
    private readonly CustomMapOSMDbContext _context;

    public ExportRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<Export?> GetByIdAsync(int exportId)
    {
        return await _context.Exports
            .FirstOrDefaultAsync(e => e.ExportId == exportId);
    }

    public async Task<Export?> GetByIdWithIncludesAsync(int exportId)
    {
        return await _context.Exports
            .Include(e => e.User)
            .Include(e => e.Membership)
            .Include(e => e.Map)
            .FirstOrDefaultAsync(e => e.ExportId == exportId);
    }

    public async Task<List<Export>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Exports
            .Include(e => e.Map)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Export>> GetByMapIdAsync(Guid mapId)
    {
        return await _context.Exports
            .Include(e => e.User)
            .Where(e => e.MapId == mapId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Export>> GetPendingExportsAsync()
    {
        return await _context.Exports
            .Where(e => e.Status == CusomMapOSM_Domain.Entities.Exports.Enums.ExportStatusEnum.Pending)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Export>> GetPendingApprovalExportsAsync()
    {
        return await _context.Exports
            .Include(e => e.User)
            .Include(e => e.Map)
            .Where(e => e.Status == CusomMapOSM_Domain.Entities.Exports.Enums.ExportStatusEnum.PendingApproval)
            .OrderBy(e => e.CompletedAt ?? e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Export> CreateAsync(Export export)
    {
        _context.Exports.Add(export);
        await _context.SaveChangesAsync();
        return export;
    }

    public async Task<bool> UpdateAsync(Export export)
    {
        _context.Exports.Update(export);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int exportId)
    {
        var export = await _context.Exports.FindAsync(exportId);
        if (export == null)
            return false;

        _context.Exports.Remove(export);
        return await _context.SaveChangesAsync() > 0;
    }
}

