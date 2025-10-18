using System;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Segments;

public class Segment
{
    public Guid SegmentId { get; set; }
    public Guid MapId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? StoryContent { get; set; }                  // Rich text/JSON from editor
    public int DisplayOrder { get; set; }
    public bool AutoFitBounds { get; set; } = true;
    public Guid? EntryAnimationPresetId { get; set; }          // Optional preset when segment starts
    public Guid? ExitAnimationPresetId { get; set; }           // Optional preset when segment ends
    public Guid? DefaultLayerAnimationPresetId { get; set; }   // Default animation for attached layers
    public SegmentPlaybackMode PlaybackMode { get; set; } = SegmentPlaybackMode.Sequential;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Map? Map { get; set; }
    public User? Creator { get; set; }
    public LayerAnimationPreset? EntryAnimationPreset { get; set; }
    public LayerAnimationPreset? ExitAnimationPreset { get; set; }
    public LayerAnimationPreset? DefaultLayerAnimationPreset { get; set; }
}
