using System;
using System.Collections.Generic;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.MapGallery;

public record MapGallerySummaryResponse
{
    public string Id { get; init; } = string.Empty;
    public Guid MapId { get; init; }
    public string MapName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? PreviewImage { get; init; }
    public MapTemplateCategoryEnum? Category { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? AuthorName { get; init; }
    public MapGalleryStatusEnum Status { get; init; }
    public bool IsFeatured { get; init; }
    public int ViewCount { get; init; }
    public int LikeCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public record MapGalleryDetailResponse
{
    public string Id { get; init; } = string.Empty;
    public Guid MapId { get; init; }
    public Guid UserId { get; init; }
    public string MapName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? PreviewImage { get; init; }
    public MapTemplateCategoryEnum? Category { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? AuthorName { get; init; }
    public string? AuthorEmail { get; init; }
    public MapGalleryStatusEnum Status { get; init; }
    public bool IsFeatured { get; init; }
    public int ViewCount { get; init; }
    public int LikeCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? RejectionReason { get; init; }
}

public record MapGallerySubmitRequest
{
    public Guid MapId { get; init; }
    public string MapName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? PreviewImage { get; init; }
    public MapTemplateCategoryEnum? Category { get; init; }
    public List<string> Tags { get; init; } = new();
}

public record MapGalleryUpdateRequest
{
    public string? MapName { get; init; }
    public string? Description { get; init; }
    public string? PreviewImage { get; init; }
    public MapTemplateCategoryEnum? Category { get; init; }
    public List<string>? Tags { get; init; }
}

public record MapGalleryApprovalRequest
{
    public MapGalleryStatusEnum Status { get; init; }
    public string? RejectionReason { get; init; }
    public bool IsFeatured { get; init; } = false;
}

