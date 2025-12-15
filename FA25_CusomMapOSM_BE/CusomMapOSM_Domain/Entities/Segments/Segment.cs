using System;
using CusomMapOSM_Domain.Entities.Animations;
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
    public string? Description { get; set; }
    public string? StoryContent { get; set; }
    public int DisplayOrder { get; set; }
    
    public string CameraState { get; set; } = string.Empty;
    
    public bool AutoAdvance { get; set; } = true;
    public int DurationMs { get; set; } = 6000;
    public bool RequireUserAction { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Map? Map { get; set; }
    public User? Creator { get; set; }
}