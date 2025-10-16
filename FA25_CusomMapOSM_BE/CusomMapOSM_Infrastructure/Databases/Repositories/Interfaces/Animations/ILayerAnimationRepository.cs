using CusomMapOSM_Domain.Entities.Animations;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;

public interface ILayerAnimationRepository
{
    // Basic CRUD operations
    Task<LayerAnimation?> GetAnimationAsync(Guid animationId, CancellationToken ct);
    Task<List<LayerAnimation>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct);
    Task AddAnimationAsync(LayerAnimation animation, CancellationToken ct);
    void UpdateAnimation(LayerAnimation animation);
    void RemoveAnimation(LayerAnimation animation);
    
    // Filtered queries
    Task<List<LayerAnimation>> GetActiveAnimationsAsync(CancellationToken ct);
    
    // Save changes
    Task<int> SaveChangesAsync(CancellationToken ct);
}
