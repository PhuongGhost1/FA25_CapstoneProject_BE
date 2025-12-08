using System.Collections.Generic;
using CusomMapOSM_Domain.Entities.Exports.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Exports;

public record CreateExportRequest
{
    public required Guid MapId { get; set; }
    public required ExportTypeEnum Format { get; set; }
    public Guid? MembershipId { get; set; } // Optional, will use active membership if not provided
    public string? ViewState { get; set; } // Optional: Current map view state (center, zoom, etc.) from frontend
    public string? MapImageData { get; set; } // Optional: Base64 encoded image data captured from frontend
    public string? SvgPathData { get; set; } // Optional: JSON string of extracted SVG path data from Leaflet layers
    public Dictionary<string, bool>? VisibleLayerIds { get; set; } // Optional: Which layers should be visible in export
    public Dictionary<string, bool>? VisibleFeatureIds { get; set; } // Optional: Which features should be visible in export
    public ExportOptions? Options { get; set; } // Optional: Export-specific options
}

public record ExportOptions
{
    public int? Width { get; set; } // For PNG/PDF exports
    public int? Height { get; set; } // For PNG/PDF exports
    public int? Dpi { get; set; } // For PNG/PDF exports
    public bool? IncludeLegend { get; set; } // Include legend in export
    public bool? IncludeScale { get; set; } // Include scale bar in export
    public string? Title { get; set; } // Custom title for export
    public string? Description { get; set; } // Custom description for export
}

public record ExportResponse
{
    public required int ExportId { get; set; }
    public required Guid MapId { get; set; }
    public string? MapName { get; set; }
    public required Guid UserId { get; set; }
    public string? UserName { get; set; }
    public required ExportTypeEnum Format { get; set; }
    public required ExportStatusEnum Status { get; set; }
    public string? FileUrl { get; set; }
    public bool CanDownload { get; set; }
    public int FileSize { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public record ExportListResponse
{
    public required List<ExportResponse> Exports { get; set; }
    public required int Total { get; set; }
}

