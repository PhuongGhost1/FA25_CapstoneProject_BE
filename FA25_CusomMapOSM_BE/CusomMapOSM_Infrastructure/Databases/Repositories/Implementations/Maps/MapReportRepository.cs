using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapReportRepository : IMapReportRepository
{
    private readonly CustomMapOSMDbContext _context;

    public MapReportRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<MapReport?> GetReportByIdAsync(Guid reportId)
    {
        return await _context.MapReports
            .Include(r => r.Map)
            .Include(r => r.ReporterUser)
            .Include(r => r.ReviewedByUser)
            .FirstOrDefaultAsync(r => r.MapReportId == reportId);
    }

    public async Task<List<MapReport>> GetReportsByMapIdAsync(Guid mapId)
    {
        return await _context.MapReports
            .Include(r => r.ReporterUser)
            .Include(r => r.ReviewedByUser)
            .Where(r => r.MapId == mapId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MapReport>> GetAllReportsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.MapReports
            .Include(r => r.Map)
            .Include(r => r.ReporterUser)
            .Include(r => r.ReviewedByUser)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<MapReport>> GetReportsByStatusAsync(int status, int page = 1, int pageSize = 20)
    {
        return await _context.MapReports
            .Include(r => r.Map)
            .Include(r => r.ReporterUser)
            .Include(r => r.ReviewedByUser)
            .Where(r => (int)r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> CreateReportAsync(MapReport report)
    {
        _context.MapReports.Add(report);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateReportAsync(MapReport report)
    {
        report.UpdatedAt = DateTime.UtcNow;
        _context.MapReports.Update(report);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetReportsCountAsync()
    {
        return await _context.MapReports.CountAsync();
    }

    public async Task<int> GetPendingReportsCountAsync()
    {
        return await _context.MapReports
            .CountAsync(r => r.Status == CusomMapOSM_Domain.Entities.Maps.Enums.MapReportStatusEnum.Pending);
    }
}

