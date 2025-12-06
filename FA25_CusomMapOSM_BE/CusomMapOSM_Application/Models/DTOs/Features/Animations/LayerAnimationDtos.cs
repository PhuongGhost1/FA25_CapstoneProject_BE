using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Models.DTOs.Features.Animations;

public record LayerAnimationDto(
    Guid LayerAnimationId,
    Guid LayerId,
    string Name,
    string SourceUrl,
    string? Coordinates,
    double RotationDeg,
    double Scale,
    int ZIndex,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive);

public record CreateLayerAnimationRequest(
    Guid LayerId,
    string Name,
    IFormFile AnimationFile,
    string? Coordinates,
    double RotationDeg,
    double Scale,
    int ZIndex);

public record UpdateLayerAnimationRequest(
    string Name,
    IFormFile AnimationFile,
    string? Coordinates,
    double RotationDeg,
    double Scale,
    int ZIndex,
    bool IsActive);
