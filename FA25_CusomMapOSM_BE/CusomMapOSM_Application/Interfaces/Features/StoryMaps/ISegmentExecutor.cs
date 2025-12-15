using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

namespace CusomMapOSM_Application.Interfaces.Features.StoryMaps;

public interface ISegmentExecutor
{
    Task<SegmentExecutionResult> ExecuteSegmentAsync(
        SegmentDto segment,
        SegmentExecutionOptions options,
        CancellationToken ct = default);

    Task<IReadOnlyList<SegmentExecutionResult>> ExecuteSegmentsAsync(
        IReadOnlyList<SegmentDto> segments,
        SegmentExecutionOptions options,
        CancellationToken ct = default);

    void StopExecution();
    void PauseExecution();
    void ResumeExecution();
    SegmentExecutionStatus GetExecutionStatus();
}
