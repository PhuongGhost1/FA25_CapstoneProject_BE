using CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspace;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Workspace;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly CustomMapOSMDbContext _context;

    public WorkspaceRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace?> GetByIdAsync(Guid id)
    {
        return await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Creator)
            .FirstOrDefaultAsync(w => w.WorkspaceId == id);
    }

    public async Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetAllAsync()
    {
        return await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Creator)
            .ToListAsync();
    }

    public async Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetByOrganizationIdAsync(Guid orgId)
    {
        return await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Creator)
            .Where(w => w.OrgId == orgId && w.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<CusomMapOSM_Domain.Entities.Workspaces.Workspace>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Creator)
            .Where(w => w.CreatedBy == userId && w.IsActive)
            .ToListAsync();
    }

    public async Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace?> GetPersonalWorkspaceAsync(Guid userId)
    {
        return await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Creator)
            .Where(w => w.CreatedBy == userId && w.OrgId == null)
            .OrderBy(w => w.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace> CreateAsync(CusomMapOSM_Domain.Entities.Workspaces.Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task<CusomMapOSM_Domain.Entities.Workspaces.Workspace> UpdateAsync(CusomMapOSM_Domain.Entities.Workspaces.Workspace workspace)
    {
        _context.Workspaces.Update(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null) return false;

        workspace.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Workspaces.AnyAsync(w => w.WorkspaceId == id);
    }

    public async Task<int> GetMapCountAsync(Guid workspaceId)
    {
        return await _context.Maps.CountAsync(m => m.WorkspaceId == workspaceId && m.IsActive);
    }
}
