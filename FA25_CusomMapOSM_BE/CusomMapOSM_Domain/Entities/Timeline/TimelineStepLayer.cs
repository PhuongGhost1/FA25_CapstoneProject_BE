using System;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Domain.Entities.Timeline;

public class TimelineStepLayer
{
    public Guid TimelineStepLayerId { get; set; }
    public Guid TimelineStepId { get; set; }
    public Guid LayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public double Opacity { get; set; } = 1.0;
    public int FadeInMs { get; set; } = 300;
    public int FadeOutMs { get; set; } = 300;
    public int DelayMs { get; set; }
    public TimelineLayerDisplayMode DisplayMode { get; set; } = TimelineLayerDisplayMode.Normal;
    public string? StyleOverride { get; set; }
    public string? Metadata { get; set; }

    public TimelineStep? TimelineStep { get; set; }
    public Layer? Layer { get; set; }
}
