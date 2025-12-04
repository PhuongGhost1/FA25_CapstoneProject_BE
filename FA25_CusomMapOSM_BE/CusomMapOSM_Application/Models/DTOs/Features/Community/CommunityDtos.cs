using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Community;

public record CommunityPostSummaryResponse
{
    public string Id { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
}

public record CommunityPostDetailResponse
{
    public string Id { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public string ContentHtml { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
    public bool IsPublished { get; init; }
}

public record CommunityPostCreateRequest
{
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public string ContentHtml { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public bool IsPublished { get; init; } = true;
}

public record CommunityPostUpdateRequest
{
    public string? Title { get; init; }
    public string? Excerpt { get; init; }
    public string? ContentHtml { get; init; }
    public string? Topic { get; init; }
    public DateTime? PublishedAt { get; init; }
    public bool? IsPublished { get; init; }
}


