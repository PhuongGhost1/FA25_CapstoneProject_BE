using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Community;

namespace CusomMapOSM_Application.Common.Mappers;

public static class CommunityMappings
{
    public static CommunityPostSummaryResponse ToSummary(this CommunityPostDocument doc) =>
        new CommunityPostSummaryResponse
        {
            Id = doc.Id,
            Slug = doc.Slug,
            Title = doc.Title,
            Excerpt = doc.Excerpt,
            Topic = doc.Topic,
            PublishedAt = doc.PublishedAt
        };

    public static CommunityPostDetailResponse ToDetail(this CommunityPostDocument doc) =>
        new CommunityPostDetailResponse
        {
            Id = doc.Id,
            Slug = doc.Slug,
            Title = doc.Title,
            Excerpt = doc.Excerpt,
            ContentHtml = doc.ContentHtml,
            Topic = doc.Topic,
            PublishedAt = doc.PublishedAt,
            IsPublished = doc.IsPublished
        };
}

