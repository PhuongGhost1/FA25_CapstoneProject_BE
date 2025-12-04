using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.MapGallery;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.MapGallery;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using MongoDB.Driver;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.MapGallery;

public class MapGalleryService : IMapGalleryService
{
    private readonly IMongoCollection<MapGalleryDocument> _collection;
    private readonly IMapService _mapService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMapRepository _mapRepository;

    public MapGalleryService(
        IMongoDatabase database,
        IMapService mapService,
        ICurrentUserService currentUserService,
        IMapRepository mapRepository, IUserRepository userRepository)
    {
        _collection = database.GetCollection<MapGalleryDocument>("map_gallery");
        _mapService = mapService;
        _currentUserService = currentUserService;
        _mapRepository = mapRepository;
        _userRepository = userRepository;
    }

    public async Task<List<MapGallerySummaryResponse>> GetPublishedMapsAsync(
        MapTemplateCategoryEnum? category,
        string? searchTerm,
        bool? featuredOnly,
        CancellationToken ct = default)
    {
        var filter = Builders<MapGalleryDocument>.Filter.Eq(x => x.Status, MapGalleryStatusEnum.Approved);

        if (category.HasValue)
        {
            filter &= Builders<MapGalleryDocument>.Filter.Eq(x => x.Category, category.Value);
        }

        if (featuredOnly == true)
        {
            filter &= Builders<MapGalleryDocument>.Filter.Eq(x => x.IsFeatured, true);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = Builders<MapGalleryDocument>.Filter.Or(
                Builders<MapGalleryDocument>.Filter.Regex(x => x.MapName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<MapGalleryDocument>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<MapGalleryDocument>.Filter.AnyIn(x => x.Tags, new[] { searchTerm })
            );
            filter &= searchFilter;
        }

        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .ToListAsync(ct);

        return docs.Select(doc => doc.ToSummary()).ToList();
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.Id == id && x.Status == MapGalleryStatusEnum.Approved)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ không tồn tại hoặc chưa được duyệt"));
        }

        // Increment view count
        await IncrementViewCountAsync(id, ct);

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByMapIdAsync(
        Guid mapId,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.MapId == mapId && x.Status == MapGalleryStatusEnum.Approved)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ không tồn tại hoặc chưa được duyệt"));
        }

        await IncrementViewCountAsync(doc.Id, ct);

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> SubmitMapAsync(
        Guid userId,
        MapGallerySubmitRequest request,
        CancellationToken ct = default)
    {
        // Check if map exists and belongs to user
        var map = await _mapRepository.GetMapById(request.MapId);
        if (map == null || !map.IsActive)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("Map.NotFound", "Bản đồ không tồn tại"));
        }

        if (map.UserId != userId)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Forbidden("MapGallery.Unauthorized", "Bạn không có quyền submit bản đồ này"));
        }

        // Check if already submitted
        var existing = await _collection
            .Find(x => x.MapId == request.MapId)
            .FirstOrDefaultAsync(ct);

        if (existing != null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Conflict("MapGallery.AlreadySubmitted", "Bản đồ này đã được submit"));
        }
        
        var userSubmit = await _userRepository.GetUserByIdAsync(userId, ct);

        var doc = new MapGalleryDocument
        {
            Id = Guid.NewGuid().ToString("N"),
            MapId = request.MapId,
            UserId = userId,
            MapName = request.MapName,
            Description = request.Description,
            PreviewImage = request.PreviewImage,
            Category = request.Category,
            Tags = request.Tags,
            AuthorName = userSubmit.FullName,
            AuthorEmail = userSubmit.Email,
            Status = MapGalleryStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(doc, cancellationToken: ct);
        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetMySubmissionAsync(
        Guid userId,
        Guid mapId,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.MapId == mapId && x.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ chưa được submit"));
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> UpdateMySubmissionAsync(
        Guid userId,
        string submissionId,
        MapGalleryUpdateRequest request,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == submissionId).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        if (doc.UserId != userId)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Forbidden("MapGallery.Unauthorized", "Bạn không có quyền chỉnh sửa submission này"));
        }

        if (doc.Status != MapGalleryStatusEnum.Pending)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Failure("MapGallery.CannotUpdate", "Chỉ có thể chỉnh sửa submission đang ở trạng thái Pending"));
        }

        if (request.MapName != null) doc.MapName = request.MapName;
        if (request.Description != null) doc.Description = request.Description;
        if (request.PreviewImage != null) doc.PreviewImage = request.PreviewImage;
        if (request.Category.HasValue) doc.Category = request.Category.Value;
        if (request.Tags != null) doc.Tags = request.Tags;
        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == submissionId, doc, cancellationToken: ct);
        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<List<MapGallerySummaryResponse>> AdminGetAllSubmissionsAsync(
        MapGalleryStatusEnum? status,
        CancellationToken ct = default)
    {
        var filter = Builders<MapGalleryDocument>.Filter.Empty;
        if (status.HasValue)
        {
            filter = Builders<MapGalleryDocument>.Filter.Eq(x => x.Status, status.Value);
        }

        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return docs.Select(doc => doc.ToSummary()).ToList();
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> AdminGetSubmissionByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> AdminApproveOrRejectAsync(
        string id,
        Guid reviewerId,
        MapGalleryApprovalRequest request,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        doc.Status = request.Status;
        doc.ReviewedBy = reviewerId;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.IsFeatured = request.IsFeatured;

        if (request.Status == MapGalleryStatusEnum.Rejected)
        {
            doc.RejectionReason = request.RejectionReason;
        }
        else if (request.Status == MapGalleryStatusEnum.Approved)
        {
            doc.PublishedAt = DateTime.UtcNow;
            doc.RejectionReason = null;
        }

        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == id, doc, cancellationToken: ct);

        // Update the actual Map entity if approved
        if (request.Status == MapGalleryStatusEnum.Approved)
        {
            // Optionally update Map.IsPublic or other fields
            // This would require calling IMapService.Update if needed
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<bool, Error>> AdminDeleteSubmissionAsync(
        string id,
        CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> IncrementViewCountAsync(
        string id,
        CancellationToken ct = default)
    {
        var update = Builders<MapGalleryDocument>.Update.Inc(x => x.ViewCount, 1);
        var result = await _collection.UpdateOneAsync(
            x => x.Id == id,
            update,
            cancellationToken: ct);

        if (result.MatchedCount == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<bool, Error>(true);
    }
}

