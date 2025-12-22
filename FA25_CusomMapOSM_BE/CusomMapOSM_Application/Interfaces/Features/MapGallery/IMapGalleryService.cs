using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.MapGallery;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.MapGallery;

public interface IMapGalleryService
{
    // Public APIs - Get approved maps
    Task<List<MapGallerySummaryResponse>> GetPublishedMapsAsync(
        MapTemplateCategoryEnum? category,
        string? searchTerm,
        bool? featuredOnly,
        bool? isStoryMap,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByIdAsync(
        string id,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByMapIdAsync(
        Guid mapId,
        CancellationToken ct = default);

    // User APIs - Submit map for gallery
    Task<Option<MapGalleryDetailResponse, Error>> SubmitMapAsync(
        Guid userId,
        MapGallerySubmitRequest request,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> GetMySubmissionAsync(
        Guid userId,
        Guid mapId,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> UpdateMySubmissionAsync(
        Guid userId,
        string submissionId,
        MapGalleryUpdateRequest request,
        CancellationToken ct = default);

    // Admin APIs
    Task<List<MapGallerySummaryResponse>> AdminGetAllSubmissionsAsync(
        MapGalleryStatusEnum? status,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> AdminGetSubmissionByIdAsync(
        string id,
        CancellationToken ct = default);

    Task<Option<MapGalleryDetailResponse, Error>> AdminApproveOrRejectAsync(
        string id,
        Guid reviewerId,
        MapGalleryApprovalRequest request,
        CancellationToken ct = default);

    Task<Option<bool, Error>> AdminDeleteSubmissionAsync(
        string id,
        CancellationToken ct = default);

    Task<Option<bool, Error>> IncrementViewCountAsync(
        string id,
        CancellationToken ct = default);

    Task<Option<bool, Error>> ToggleLikeAsync(
        string id,
        Guid userId,
        CancellationToken ct = default);

    // Duplicate map from gallery
    Task<Option<MapGalleryDuplicateResponse, Error>> DuplicateMapFromGalleryAsync(
        Guid userId,
        string galleryId,
        MapGalleryDuplicateRequest request,
        CancellationToken ct = default);
}

