using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Animations;

public class LayerAnimationRepository : ILayerAnimationRepository
{
    private readonly CustomMapOSMDbContext _db;

    public LayerAnimationRepository(CustomMapOSMDbContext db)
    {
        _db = db;
    }

    public async Task<LayerAnimation?> GetAnimationAsync(Guid animationId, CancellationToken ct)
    {
        return await _db.LayerAnimations.FirstOrDefaultAsync(a => a.LayerAnimationId == animationId, ct);
    }

    public async Task<List<LayerAnimation>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct)
    {
        return await _db.LayerAnimations.Where(a => a.LayerId == layerId).OrderBy(a => a.ZIndex).ToListAsync(ct);
    }

    public async Task AddAnimationAsync(LayerAnimation animation, CancellationToken ct)
    {
        await _db.LayerAnimations.AddAsync(animation, ct);
    }

    public void UpdateAnimation(LayerAnimation animation)
    {
        _db.LayerAnimations.Update(animation);
    }

    public void RemoveAnimation(LayerAnimation animation)
    {
        _db.LayerAnimations.Remove(animation);
    }

    public async Task<List<LayerAnimation>> GetActiveAnimationsAsync(CancellationToken ct)
    {
        return await _db.LayerAnimations.Where(a => a.IsActive).ToListAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _db.SaveChangesAsync(ct);
    }
}

