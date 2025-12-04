using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.MapGallery;

namespace CusomMapOSM_Application.Common.Mappers;

public static class MapGalleryMappings
{
    public static MapGallerySummaryResponse ToSummary(this MapGalleryDocument doc) =>
        new MapGallerySummaryResponse
        {
            Id = doc.Id,
            MapId = doc.MapId,
            MapName = doc.MapName,
            Description = doc.Description,
            PreviewImage = doc.PreviewImage,
            Category = doc.Category,
            Tags = doc.Tags,
            AuthorName = doc.AuthorName,
            Status = doc.Status,
            IsFeatured = doc.IsFeatured,
            ViewCount = doc.ViewCount,
            LikeCount = doc.LikeCount,
            CreatedAt = doc.CreatedAt,
            PublishedAt = doc.PublishedAt
        };

    public static MapGalleryDetailResponse ToDetail(this MapGalleryDocument doc) =>
        new MapGalleryDetailResponse
        {
            Id = doc.Id,
            MapId = doc.MapId,
            UserId = doc.UserId,
            MapName = doc.MapName,
            Description = doc.Description,
            PreviewImage = doc.PreviewImage,
            Category = doc.Category,
            Tags = doc.Tags,
            AuthorName = doc.AuthorName,
            AuthorEmail = doc.AuthorEmail,
            Status = doc.Status,
            IsFeatured = doc.IsFeatured,
            ViewCount = doc.ViewCount,
            LikeCount = doc.LikeCount,
            CreatedAt = doc.CreatedAt,
            PublishedAt = doc.PublishedAt,
            ReviewedAt = doc.ReviewedAt,
            RejectionReason = doc.RejectionReason
        };
}

