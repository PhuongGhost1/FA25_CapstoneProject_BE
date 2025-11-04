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

    public async Task<AnimatedLayer?> GetAnimationAsync(Guid animationId, CancellationToken ct)
    {
        return await _db.AnimatedLayers.FirstOrDefaultAsync(a => a.AnimatedLayerId == animationId, ct);
    }

    public async Task<List<AnimatedLayer>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct)
    {
        return await _db.AnimatedLayers.Where(a => a.LayerId == layerId).OrderBy(a => a.ZIndex).ToListAsync(ct);
    }

    public async Task AddAnimationAsync(AnimatedLayer animation, CancellationToken ct)
    {
        await _db.AnimatedLayers.AddAsync(animation, ct);
    }

    public void UpdateAnimation(AnimatedLayer animation)
    {
        _db.AnimatedLayers.Update(animation);
    }

    public void RemoveAnimation(AnimatedLayer animation)
    {
        _db.AnimatedLayers.Remove(animation);
    }

    public async Task<List<AnimatedLayer>> GetActiveAnimationsAsync(CancellationToken ct)
    {
        return await _db.AnimatedLayers.Where(a => a.IsVisible).ToListAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _db.SaveChangesAsync(ct);
    }
}

