using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.Community;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Community;
using MongoDB.Driver;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Community;

public class CommunityService : ICommunityService
{
    private readonly IMongoCollection<CommunityPostDocument> _collection;

    public CommunityService(IMongoDatabase database)
    {
        _collection = database.GetCollection<CommunityPostDocument>("community_posts");
    }

    public async Task<List<CommunityPostSummaryResponse>> GetPublishedPostsAsync(string? topic, CancellationToken ct = default)
    {
        var filter = Builders<CommunityPostDocument>.Filter.Eq(x => x.IsPublished, true);
        if (!string.IsNullOrWhiteSpace(topic))
        {
            filter &= Builders<CommunityPostDocument>.Filter.Eq(x => x.Topic, topic);
        }

        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.PublishedAt)
            .ToListAsync(ct);

        return docs.Select(doc => doc.ToSummary()).ToList();
    }

    public async Task<Option<CommunityPostDetailResponse, Error>> GetPostBySlugAsync(string slug, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.Slug == slug && x.IsPublished)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<CommunityPostDetailResponse, Error>(
                Error.NotFound("Community.PostNotFound", "Bài viết không tồn tại"));
        }

        return Option.Some<CommunityPostDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<List<CommunityPostSummaryResponse>> AdminGetPostsAsync(CancellationToken ct = default)
    {
        var docs = await _collection
            .Find(Builders<CommunityPostDocument>.Filter.Empty)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return docs.Select(doc => doc.ToSummary()).ToList();
    }

    public async Task<Option<CommunityPostDetailResponse, Error>> AdminGetPostByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<CommunityPostDetailResponse, Error>(
                Error.NotFound("Community.PostNotFound", "Bài viết không tồn tại"));
        }

        return Option.Some<CommunityPostDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<CommunityPostDetailResponse, Error>> AdminCreatePostAsync(CommunityPostCreateRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return Option.None<CommunityPostDetailResponse, Error>(
                Error.Failure("Community.SlugRequired", "Slug không được để trống"));
        }

        var exists = await _collection.Find(x => x.Slug == request.Slug).AnyAsync(ct);
        if (exists)
        {
            return Option.None<CommunityPostDetailResponse, Error>(
                Error.Conflict("Community.SlugExists", "Slug đã tồn tại"));
        }

        var doc = new CommunityPostDocument
        {
            Id = Guid.NewGuid().ToString("N"),
            Slug = request.Slug,
            Title = request.Title,
            Excerpt = request.Excerpt,
            ContentHtml = request.ContentHtml,
            Topic = request.Topic,
            PublishedAt = request.PublishedAt ?? DateTime.UtcNow,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(doc, cancellationToken: ct);
        return Option.Some<CommunityPostDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<CommunityPostDetailResponse, Error>> AdminUpdatePostAsync(string id, CommunityPostUpdateRequest request, CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<CommunityPostDetailResponse, Error>(
                Error.NotFound("Community.PostNotFound", "Bài viết không tồn tại"));
        }

        if (request.Title != null) doc.Title = request.Title;
        if (request.Excerpt != null) doc.Excerpt = request.Excerpt;
        if (request.ContentHtml != null) doc.ContentHtml = request.ContentHtml;
        if (request.Topic != null) doc.Topic = request.Topic;
        if (request.PublishedAt.HasValue) doc.PublishedAt = request.PublishedAt.Value;
        if (request.IsPublished.HasValue) doc.IsPublished = request.IsPublished.Value;

        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == id, doc, cancellationToken: ct);
        return Option.Some<CommunityPostDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<bool, Error>> AdminDeletePostAsync(string id, CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Community.PostNotFound", "Bài viết không tồn tại"));
        }

        return Option.Some<bool, Error>(true);
    }
}