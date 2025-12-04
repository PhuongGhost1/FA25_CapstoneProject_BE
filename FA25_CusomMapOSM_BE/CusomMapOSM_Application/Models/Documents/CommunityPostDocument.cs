using System;

namespace CusomMapOSM_Application.Models.Documents;

public class CommunityPostDocument
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty; // Education, Tutorial, Product, Stories, Business
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;
    public Guid? AuthorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


