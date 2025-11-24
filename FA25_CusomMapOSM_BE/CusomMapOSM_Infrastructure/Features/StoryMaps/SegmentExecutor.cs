using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using System.Diagnostics;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

public class SegmentExecutor : ISegmentExecutor
{
    private readonly IStoryMapRepository _repository;
    private readonly ISegmentExecutionStateStore _stateStore;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private SegmentExecutionStatus _status = SegmentExecutionStatus.Idle;
    private readonly object _statusLock = new();
    private readonly ManualResetEventSlim _pauseEvent = new(true);

    public SegmentExecutor(IStoryMapRepository repository, ISegmentExecutionStateStore stateStore)
    {
        _repository = repository;
        _stateStore = stateStore;
    }

    public async Task<SegmentExecutionResult> ExecuteSegmentAsync(
        SegmentDto segment,
        SegmentExecutionOptions options,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var executedComponents = new List<ExecutedComponent>();
        var combinedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, _cancellationTokenSource.Token).Token;

        try
        {
            SetStatus(SegmentExecutionStatus.Running);

            var executionOrder = options.CustomOrder ?? new SegmentExecutionOrder();
            var componentOrder = GetComponentExecutionOrder(executionOrder);
            for (int i = 0; i < componentOrder.Count; i++)
            {
                await WaitForResumeAsync(combinedCt);
                var (componentType, order) = componentOrder[i];
                
                if (combinedCt.IsCancellationRequested)
                    break;

                var componentResult = await ExecuteComponentAsync(segment, componentType, options, combinedCt);
                executedComponents.Add(componentResult);

                _stateStore.Set(segment.MapId, new SegmentExecutionCheckpoint
                {
                    MapId = segment.MapId,
                    SegmentId = segment.SegmentId,
                    SegmentIndex = segment.DisplayOrder,
                    ComponentType = componentType.ToString(),
                    ComponentIndex = i,
                    OptionsSnapshot = options
                });
                if (i < componentOrder.Count - 1 && options.DefaultDelayMs > 0)
                {
                    await DelayWithPauseAsync(options.DefaultDelayMs, combinedCt);
                }
            }

            stopwatch.Stop();

            return new SegmentExecutionResult
            {
                Segment = segment,
                IsSuccess = executedComponents.All(c => c.IsSuccess),
                Duration = stopwatch.Elapsed,
                ExecutedComponents = executedComponents
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            SetStatus(SegmentExecutionStatus.Stopped);
            return new SegmentExecutionResult
            {
                Segment = segment,
                IsSuccess = false,
                ErrorMessage = "Execution was cancelled",
                Duration = stopwatch.Elapsed,
                ExecutedComponents = executedComponents
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            SetStatus(SegmentExecutionStatus.Error);
            return new SegmentExecutionResult
            {
                Segment = segment,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed,
                ExecutedComponents = executedComponents
            };
        }
        finally
        {
            if (_status == SegmentExecutionStatus.Running)
            {
                SetStatus(SegmentExecutionStatus.Idle);
                _stateStore.Reset(segment.MapId);
            }
        }
    }

    public async Task<IReadOnlyList<SegmentExecutionResult>> ExecuteSegmentsAsync(
        IReadOnlyList<SegmentDto> segments, 
        SegmentExecutionOptions options, 
        CancellationToken ct = default)
    {
        var results = new List<SegmentExecutionResult>();

        foreach (var segment in segments.OrderBy(s => s.DisplayOrder))
        {
            if (ct.IsCancellationRequested)
                break;

            var result = await ExecuteSegmentAsync(segment, options, ct);
            results.Add(result);

            // If execution failed and auto-advance is disabled, stop
            if (!result.IsSuccess && !options.AutoAdvance)
                break;

            // Add delay between segments
            if (options.DefaultDelayMs > 0 && segment != segments.Last())
            {
                await Task.Delay(options.DefaultDelayMs, ct);
            }
        }

        return results;
    }

    public void StopExecution()
    {
        _cancellationTokenSource.Cancel();
        SetStatus(SegmentExecutionStatus.Stopped);
    }

    public void PauseExecution()
    {
        SetStatus(SegmentExecutionStatus.Paused);
        _pauseEvent.Reset();
    }

    public void ResumeExecution()
    {
        if (_status == SegmentExecutionStatus.Paused)
        {
            SetStatus(SegmentExecutionStatus.Running);
            _pauseEvent.Set();
        }
    }

    public SegmentExecutionStatus GetExecutionStatus()
    {
        lock (_statusLock)
        {
            return _status;
        }
    }

    private async Task<ExecutedComponent> ExecuteComponentAsync(
        SegmentDto segment, 
        ComponentType componentType, 
        SegmentExecutionOptions options, 
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var componentId = Guid.Empty;
        var componentName = string.Empty;
        var isSuccess = false;
        var errorMessage = (string?)null;

        try
        {
            await WaitForResumeAsync(ct);
            switch (componentType)
            {
                case ComponentType.POI:
                    if (options.ShowPOIs)
                    {
                        await ExecutePOIsAsync(segment, ct);
                        componentId = segment.SegmentId;
                        componentName = $"POIs ({segment.Locations.Count})";
                        isSuccess = true;
                    }
                    break;

                case ComponentType.Zone:
                    if (options.ShowZones)
                    {
                        await ExecuteZonesAsync(segment, ct);
                        componentId = segment.SegmentId;
                        componentName = $"Zones ({segment.Zones.Count})";
                        isSuccess = true;
                    }
                    break;

                case ComponentType.Layer:
                    if (options.AnimateLayers)
                    {
                        await ExecuteLayersAsync(segment, options, ct);
                        componentId = segment.SegmentId;
                        componentName = $"Layers ({segment.Layers.Count})";
                        isSuccess = true;
                    }
                    break;

                case ComponentType.Timeline:
                    if (options.ExecuteTimeline)
                    {
                        await ExecuteTimelineAsync(segment, ct);
                        componentId = segment.SegmentId;
                        componentName = "Timeline Steps";
                        isSuccess = true;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            isSuccess = false;
        }
        finally
        {
            stopwatch.Stop();
        }

        return new ExecutedComponent
        {
            Type = componentType,
            ComponentId = componentId,
            Name = componentName,
            Order = GetComponentOrder(componentType),
            Duration = stopwatch.Elapsed,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };
    }

    private async Task ExecutePOIsAsync(SegmentDto segment, CancellationToken ct)
    {
        await DelayWithPauseAsync(500, ct);
    }

    private async Task ExecuteZonesAsync(SegmentDto segment, CancellationToken ct)
    {
        await DelayWithPauseAsync(800, ct);
    }

    private async Task ExecuteLayersAsync(SegmentDto segment, SegmentExecutionOptions options, CancellationToken ct)
    {
        var animationDuration = options.DefaultAnimationDurationMs;
        await DelayWithPauseAsync(animationDuration, ct);
    }

    private async Task ExecuteTimelineAsync(SegmentDto segment, CancellationToken ct)
    {
        // Get timeline transitions for this segment's map
        var transitions = await _repository.GetTimelineTransitionsByMapAsync(segment.MapId, ct);
        
        // Find transitions that involve this segment (either as FromSegment or ToSegment)
        var relevantTransitions = transitions
            .Where(t => t.FromSegmentId == segment.SegmentId || t.ToSegmentId == segment.SegmentId)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        // Execute each transition
        foreach (var transition in relevantTransitions)
        {
            await ExecuteTransitionAsync(transition, ct);
        }
    }

    private async Task ExecuteTransitionAsync(CusomMapOSM_Domain.Entities.Timeline.TimelineTransition transition, CancellationToken ct)
    {
        // Execute camera animation if enabled
        if (transition.AnimateCamera)
        {
            var animationDuration = transition.CameraAnimationDurationMs > 0 
                ? transition.CameraAnimationDurationMs 
                : 1500;
            await DelayWithPauseAsync(animationDuration, ct);
        }

        // Show overlay if enabled
        if (transition.ShowOverlay && !string.IsNullOrEmpty(transition.OverlayContent))
        {
            var overlayDuration = transition.DurationMs > 0 
                ? transition.DurationMs 
                : 1000;
            await DelayWithPauseAsync(overlayDuration, ct);
        }

        // Wait for user action if required
        if (transition.RequireUserAction)
        {
            // In a real implementation, this would wait for user input
            // For now, just add a default wait time
            await DelayWithPauseAsync(2000, ct);
        }
    }

    // Legacy method - kept for backward compatibility but not actively used
    private async Task ExecuteTimelineStepAsync(TimelineStepDto step, CancellationToken ct)
    {
        var duration = step.DurationMs > 0 ? step.DurationMs : 1000;
        await DelayWithPauseAsync(duration, ct);
    }

    private List<(ComponentType Type, int Order)> GetComponentExecutionOrder(SegmentExecutionOrder order)
    {
        var components = new List<(ComponentType, int)>
        {
            (ComponentType.POI, order.POIOrder),
            (ComponentType.Zone, order.ZoneOrder),
            (ComponentType.Layer, order.LayerOrder),
            (ComponentType.Timeline, order.TimelineOrder)
        };

        return components.OrderBy(c => c.Item2).ToList();
    }

    private int GetComponentOrder(ComponentType componentType)
    {
        return componentType switch
        {
            ComponentType.POI => 1,
            ComponentType.Zone => 2,
            ComponentType.Layer => 3,
            ComponentType.Timeline => 4,
            ComponentType.Animation => 5,
            _ => 0
        };
    }

    private void SetStatus(SegmentExecutionStatus status)
    {
        lock (_statusLock)
        {
            _status = status;
        }
    }

    private async Task WaitForResumeAsync(CancellationToken ct)
    {
        while (_status == SegmentExecutionStatus.Paused)
        {
            _pauseEvent.Wait(50, ct);
            await Task.Yield();
        }
    }

    private async Task DelayWithPauseAsync(int totalMs, CancellationToken ct)
    {
        var remaining = totalMs;
        const int slice = 100;
        while (remaining > 0)
        {
            await WaitForResumeAsync(ct);
            var wait = Math.Min(slice, remaining);
            await Task.Delay(wait, ct);
            remaining -= wait;
        }
    }
}
