using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapLayer
{
    public Guid MapLayerId { get; set; }
    public Guid MapId { get; set; }
    public Guid LayerId { get; set; }
    public string LayerName { get; set; } = string.Empty;          
    public int LayerTypeId { get; set; }                           
    public Guid SourceId { get; set; }                           
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    public int LayerOrder { get; set; } = 0;
    public string? LayerData { get; set; }                    
    public string? LayerStyle { get; set; }                     
    public string? CustomStyle { get; set; }                    
    public string? FilterConfig { get; set; }
    public int? FeatureCount { get; set; }
    public double? DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Map? Map { get; set; }
    public Layer? Layer { get; set; }
}
