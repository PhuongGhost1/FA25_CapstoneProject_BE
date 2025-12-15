using CusomMapOSM_Domain.Entities.Animations;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;

public interface ILayerAnimationRepository
{
    // Basic CRUD operations
    Task<AnimatedLayer?> GetAnimationAsync(Guid animationId, CancellationToken ct);
    Task<List<AnimatedLayer>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct);
    Task AddAnimationAsync(AnimatedLayer animation, CancellationToken ct);
    void UpdateAnimation(AnimatedLayer animation);
    void RemoveAnimation(AnimatedLayer animation);
    
    // Filtered queries
    Task<List<AnimatedLayer>> GetActiveAnimationsAsync(CancellationToken ct);
    
    // Save changes
    Task<int> SaveChangesAsync(CancellationToken ct);
}
