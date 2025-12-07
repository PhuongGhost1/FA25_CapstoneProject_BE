using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Layers;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Animations;

public class LayerAnimationService : ILayerAnimationService
{
    private readonly ILayerAnimationRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly IUserAssetService _userAssetService;
    private readonly ILayerRepository _layerRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IStoryMapRepository _storyMapRepository;

    public LayerAnimationService(
        ILayerAnimationRepository repository, 
        ICurrentUserService currentUserService, 
        IFirebaseStorageService firebaseStorageService, 
        IUserAssetService userAssetService,
        ILayerRepository layerRepository,
        IWorkspaceRepository workspaceRepository,
        IStoryMapRepository storyMapRepository)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _firebaseStorageService = firebaseStorageService;
        _userAssetService = userAssetService;
        _layerRepository = layerRepository;
        _workspaceRepository = workspaceRepository;
        _storyMapRepository = storyMapRepository;
    }

    private async Task<Guid?> GetOrganizationIdAsync(Guid layerId, CancellationToken ct)
    {
        var layer = await _layerRepository.GetLayerByIdAsync(layerId, ct);
        if (layer == null) return null;

        var map = await _storyMapRepository.GetMapAsync(layer.MapId, ct);
        if (map == null || !map.WorkspaceId.HasValue) return null;

        var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
        return workspace?.OrgId;
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
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<LayerAnimationDto, Error>(Error.Unauthorized("Animation.Unauthorized", "User not authenticated"));
        }
        string sourceUrl = string.Empty;
        var orgId = await GetOrganizationIdAsync(request.LayerId, ct);
        if (request.AnimationFile is not null)
        {   
            using var stream = request.AnimationFile.OpenReadStream();
            var animationUrl = await _firebaseStorageService.UploadFileAsync(request.AnimationFile.FileName, stream ,"animations");
            if (animationUrl is null)
            {
                return Option.None<LayerAnimationDto, Error>(Error.NotFound("Animation.AnimationFile.NotFound", "Animation file not found"));
            }
            sourceUrl = animationUrl;

            // Register in User Library
            try 
            {
                await _userAssetService.CreateAssetMetadataAsync(
                    currentUserId.Value,
                    request.AnimationFile.FileName,
                    animationUrl,
                    "image", // Assuming static images/gifs are images. if "video", might be different but usually animations here are images.
                    request.AnimationFile.Length,
                    request.AnimationFile.ContentType,
                    orgId);
            }
            catch (Exception) { /* Ensure robust */ }
        }
        var entity = new AnimatedLayer
        {
            AnimatedLayerId = Guid.NewGuid(),
            LayerId = request.LayerId,
            CreatedBy = currentUserId.Value,
            Name = request.Name,
            SourceUrl = sourceUrl,
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
        if (request.AnimationFile is not null)
        {
            using var stream = request.AnimationFile.OpenReadStream();
            var animationUrl = await _firebaseStorageService.UploadFileAsync(request.AnimationFile.FileName, stream, "animations");
            if (animationUrl is null)
            {
                return Option.None<LayerAnimationDto, Error>(Error.NotFound("Animation.AnimationFile.NotFound", "Animation file not found"));
            }
            entity.SourceUrl = animationUrl;

            // Register in User Library
            var currentUserId = _currentUserService.GetUserId();
            var orgId = await GetOrganizationIdAsync(entity.LayerId.Value, ct);
            if (currentUserId.HasValue)
            {
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        currentUserId.Value,
                        request.AnimationFile.FileName,
                        animationUrl,
                        "image",
                        request.AnimationFile.Length,
                        request.AnimationFile.ContentType,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }
        }
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

