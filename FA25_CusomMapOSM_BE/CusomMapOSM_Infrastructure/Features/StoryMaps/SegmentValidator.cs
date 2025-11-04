using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

/// <summary>
/// Validator for StoryMap Segment operations
/// </summary>
public static class SegmentValidator
{
    /// <summary>
    /// Validates UpdateSegmentRequest
    /// </summary>
    public static Option<bool, Error> ValidateUpdateRequest(UpdateSegmentRequest request)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.InvalidName", 
                    "Segment name is required and cannot be empty"));
        }

        if (request.Name.Length > 200)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.NameTooLong", 
                    "Segment name must not exceed 200 characters"));
        }

        // Validate display order if provided
        if (request.DisplayOrder.HasValue && request.DisplayOrder.Value < 0)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.InvalidDisplayOrder", 
                    "Display order must be non-negative"));
        }

        // Validate camera state JSON if provided
        if (!string.IsNullOrWhiteSpace(request.CameraState))
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(request.CameraState);
                
                // Validate required camera properties
                if (!doc.RootElement.TryGetProperty("center", out _))
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("StoryMap.Segment.MissingCameraCenter", 
                            "Camera state must include 'center' property"));
                }
                
                if (!doc.RootElement.TryGetProperty("zoom", out _))
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("StoryMap.Segment.MissingCameraZoom", 
                            "Camera state must include 'zoom' property"));
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                return Option.None<bool, Error>(
                    Error.ValidationError("StoryMap.Segment.InvalidCameraState", 
                        $"Camera state must be valid JSON: {ex.Message}"));
            }
        }

        // Validate duration if provided
        if (request.DurationMs.HasValue)
        {
            if (request.DurationMs.Value < 0)
            {
                return Option.None<bool, Error>(
                    Error.ValidationError("StoryMap.Segment.InvalidDuration", 
                        "Duration must be non-negative"));
            }

            if (request.DurationMs.Value > 300000) // Max 5 minutes
            {
                return Option.None<bool, Error>(
                    Error.ValidationError("StoryMap.Segment.DurationTooLong", 
                        "Duration must not exceed 300000ms (5 minutes)"));
            }
        }

        // Validate story content length if provided
        if (request.StoryContent != null && request.StoryContent.Length > 10000)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.StoryContentTooLong", 
                    "Story content must not exceed 10000 characters"));
        }

        // Validate summary length if provided
        if (request.Description != null && request.Description.Length > 500)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.SummaryTooLong", 
                    "Summary must not exceed 500 characters"));
        }

        // Validate playback mode consistency
        if (request.PlaybackMode.HasValue)
        {
            var playbackMode = request.PlaybackMode.Value;
            
            // If Manual mode, auto advance should be false
            if (playbackMode == CusomMapOSM_Domain.Entities.Segments.Enums.SegmentPlaybackMode.Manual)
            {
                if (request.AutoAdvance.HasValue && request.AutoAdvance.Value)
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("StoryMap.Segment.InconsistentPlayback", 
                            "Manual playback mode requires AutoAdvance to be false"));
                }
            }
            
            // If Auto mode, duration is required
            if (playbackMode == CusomMapOSM_Domain.Entities.Segments.Enums.SegmentPlaybackMode.Auto)
            {
                if (request.DurationMs.HasValue && request.DurationMs.Value <= 0)
                {
                    return Option.None<bool, Error>(
                        Error.ValidationError("StoryMap.Segment.AutoRequiresDuration", 
                            "Auto playback mode requires a positive duration"));
                }
            }
        }

        return Option.Some<bool, Error>(true);
    }

    /// <summary>
    /// Validates CreateSegmentRequest
    /// </summary>
    public static Option<bool, Error> ValidateCreateRequest(CreateSegmentRequest request)
    {
        // Similar validation as Update but all required fields must be present
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.InvalidName", 
                    "Segment name is required"));
        }

        if (request.Name.Length > 200)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.NameTooLong", 
                    "Segment name must not exceed 200 characters"));
        }

        if (request.DisplayOrder < 0)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("StoryMap.Segment.InvalidDisplayOrder", 
                    "Display order must be non-negative"));
        }

        return Option.Some<bool, Error>(true);
    }
}
