using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Animations;

public class LayerAnimationService : ILayerAnimationService
{
    private readonly ILayerAnimationRepository _repository;

    public LayerAnimationService(ILayerAnimationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Option<IReadOnlyCollection<LayerAnimationDto>, Error>> GetAnimationsByLayerAsync(Guid layerId, CancellationToken ct = default)
    {
        var items = await _repository.GetAnimationsByLayerAsync(layerId, ct);
        var dtos = items.Select(a => a.ToLayerAnimationDto()).ToList();
        return Option.Some<IReadOnlyCollection<LayerAnimationDto>, Error>(dtos);
    }

    public async Task<Option<LayerAnimationDto, Error>> GetAnimationAsync(Guid animationId, CancellationToken ct = default)
    {
        var item = await _repository.GetAnimationAsync(animationId, ct);
        if (item is null)
        {
            return Option.None<LayerAnimationDto, Error>(Error.NotFound("Animation.NotFound", "Animation not found"));
        }
        return Option.Some<LayerAnimationDto, Error>(item.ToLayerAnimationDto());
    }

    public async Task<Option<LayerAnimationDto, Error>> CreateAnimationAsync(CreateLayerAnimationRequest request, CancellationToken ct = default)
    {
        var entity = new AnimatedLayer
        {
            AnimatedLayerId = Guid.NewGuid(),
            LayerId = request.LayerId,
            CreatedBy = Guid.Empty, // TODO: Get from current user context
            Name = request.Name,
            SourceUrl = request.SourceUrl,
            Coordinates = request.Coordinates,
            RotationDeg = request.RotationDeg,
            Scale = request.Scale,
            ZIndex = request.ZIndex,
            CreatedAt = DateTime.UtcNow,
            IsVisible = true
        };

        await _repository.AddAnimationAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<LayerAnimationDto, Error>(entity.ToLayerAnimationDto());
    }

    public async Task<Option<LayerAnimationDto, Error>> UpdateAnimationAsync(Guid animationId, UpdateLayerAnimationRequest request, CancellationToken ct = default)
    {
        var entity = await _repository.GetAnimationAsync(animationId, ct);
        if (entity is null)
        {
            return Option.None<LayerAnimationDto, Error>(Error.NotFound("Animation.NotFound", "Animation not found"));
        }

        entity.Name = request.Name;
        entity.SourceUrl = request.SourceUrl;
        entity.Coordinates = request.Coordinates;
        entity.RotationDeg = request.RotationDeg;
        entity.Scale = request.Scale;
        entity.ZIndex = request.ZIndex;
        entity.IsVisible = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateAnimation(entity);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<LayerAnimationDto, Error>(entity.ToLayerAnimationDto());
    }

    public async Task<Option<bool, Error>> DeleteAnimationAsync(Guid animationId, CancellationToken ct = default)
    {
        var entity = await _repository.GetAnimationAsync(animationId, ct);
        if (entity is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Animation.NotFound", "Animation not found"));
        }
        _repository.RemoveAnimation(entity);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<IReadOnlyCollection<LayerAnimationDto>, Error>> GetActiveAnimationsAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetActiveAnimationsAsync(ct);
        var dtos = items.Select(a => a.ToLayerAnimationDto()).ToList();
        return Option.Some<IReadOnlyCollection<LayerAnimationDto>, Error>(dtos);
    }


}

