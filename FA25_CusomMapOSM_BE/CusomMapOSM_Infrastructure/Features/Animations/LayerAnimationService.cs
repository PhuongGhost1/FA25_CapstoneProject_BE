using CusomMapOSM_Application.Common.Errors;
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
        var dtos = items.Select(ToDto).ToList();
        return Option.Some<IReadOnlyCollection<LayerAnimationDto>, Error>(dtos);
    }

    public async Task<Option<LayerAnimationDto, Error>> GetAnimationAsync(Guid animationId, CancellationToken ct = default)
    {
        var item = await _repository.GetAnimationAsync(animationId, ct);
        if (item is null)
        {
            return Option.None<LayerAnimationDto, Error>(Error.NotFound("Animation.NotFound", "Animation not found"));
        }
        return Option.Some<LayerAnimationDto, Error>(ToDto(item));
    }

    public async Task<Option<LayerAnimationDto, Error>> CreateAnimationAsync(CreateLayerAnimationRequest request, CancellationToken ct = default)
    {
        var entity = new LayerAnimation
        {
            LayerAnimationId = Guid.NewGuid(),
            LayerId = request.LayerId,
            Name = request.Name,
            SourceUrl = request.SourceUrl,
            Coordinates = request.Coordinates,
            RotationDeg = request.RotationDeg,
            Scale = request.Scale,
            ZIndex = request.ZIndex,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddAnimationAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<LayerAnimationDto, Error>(ToDto(entity));
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
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateAnimation(entity);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<LayerAnimationDto, Error>(ToDto(entity));
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
        var dtos = items.Select(ToDto).ToList();
        return Option.Some<IReadOnlyCollection<LayerAnimationDto>, Error>(dtos);
    }

    private static LayerAnimationDto ToDto(LayerAnimation a)
    {
        return new LayerAnimationDto(
            a.LayerAnimationId,
            a.LayerId,
            a.Name,
            a.SourceUrl,
            a.Coordinates,
            a.RotationDeg,
            a.Scale,
            a.ZIndex,
            a.CreatedAt,
            a.UpdatedAt,
            a.IsActive
        );
    }
}

