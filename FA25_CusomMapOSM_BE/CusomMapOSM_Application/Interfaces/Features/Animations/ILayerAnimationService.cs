using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Animations;

public interface ILayerAnimationService
{
    // Basic CRUD operations (minimal model)
    Task<Option<IReadOnlyCollection<LayerAnimationDto>, Error>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct = default);
    Task<Option<LayerAnimationDto, Error>> GetAnimationAsync(Guid animationId, CancellationToken ct = default);
    Task<Option<LayerAnimationDto, Error>> CreateAnimationAsync(CreateLayerAnimationRequest request, CancellationToken ct = default);
    Task<Option<LayerAnimationDto, Error>> UpdateAnimationAsync(Guid animationId, UpdateLayerAnimationRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteAnimationAsync(Guid animationId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<LayerAnimationDto>, Error>> GetActiveAnimationsAsync(CancellationToken ct = default);
}
