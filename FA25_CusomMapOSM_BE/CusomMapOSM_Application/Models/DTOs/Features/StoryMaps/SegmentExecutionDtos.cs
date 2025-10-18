using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

namespace CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

public record SegmentExecutionOptions
{
    public bool AutoAdvance { get; init; } = true;
    public bool ShowPOIs { get; init; } = true;
    public bool ShowZones { get; init; } = true;
    public bool AnimateLayers { get; init; } = true;
    public bool ExecuteTimeline { get; init; } = true;
    public int DefaultDelayMs { get; init; } = 1000;
    public int DefaultAnimationDurationMs { get; init; } = 2000;
    public SegmentExecutionOrder? CustomOrder { get; init; }
}

public record SegmentExecutionOrder
{
    public int POIOrder { get; init; } = 1;
    public int ZoneOrder { get; init; } = 2;
    public int LayerOrder { get; init; } = 3;
    public int TimelineOrder { get; init; } = 4;
}

public record SegmentExecutionResult
{
    public SegmentDto Segment { get; init; } = null!;
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
    public IReadOnlyList<ExecutedComponent> ExecutedComponents { get; init; } = Array.Empty<ExecutedComponent>();
}

public record ExecutedComponent
{
    public ComponentType Type { get; init; }
    public Guid ComponentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public TimeSpan Duration { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}

public enum ComponentType
{
    POI = 1,
    Zone = 2,
    Layer = 3,
    Timeline = 4,
    Animation = 5
}

public enum SegmentExecutionStatus
{
    Idle = 0,
    Running = 1,
    Paused = 2,
    Stopped = 3,
    Error = 4
}

public record ExecutedComponentDto
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required int Order { get; set; }
    public required double Duration { get; set; }
    public required bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public record SegmentExecutionResultDto
{
    public required bool IsSuccess { get; set; }
    public required string Message { get; set; }
    public required double Duration { get; set; }
    public required IReadOnlyCollection<ExecutedComponentDto> ExecutedComponents { get; set; }
}

public record ExecuteSegmentResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public required double Duration { get; set; }
    public required IReadOnlyCollection<ExecutedComponentDto> ExecutedComponents { get; set; }
}

public record SegmentExecutionSummaryDto
{
    public required Guid SegmentId { get; set; }
    public required string SegmentName { get; set; }
    public required bool Success { get; set; }
    public required double Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public required IReadOnlyCollection<ExecutedComponentDto> ExecutedComponents { get; set; }
}

public record ExecuteAllSegmentsResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public required int TotalSegments { get; set; }
    public required double TotalDuration { get; set; }
    public required IReadOnlyCollection<SegmentExecutionSummaryDto> Results { get; set; }
}

public record ExecutionStatusDto
{
    public required string Status { get; set; }
    public required bool IsRunning { get; set; }
    public required bool IsPaused { get; set; }
    public required bool IsIdle { get; set; }
}

public record ExecutionControlResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
}

public record SegmentExecutionCheckpoint
{
    public required Guid MapId { get; set; }
    public Guid? SegmentId { get; set; }
    public int SegmentIndex { get; set; }
    public string? ComponentType { get; set; }
    public int ComponentIndex { get; set; }
    public int? ElapsedMs { get; set; }
    public SegmentExecutionOptions? OptionsSnapshot { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
