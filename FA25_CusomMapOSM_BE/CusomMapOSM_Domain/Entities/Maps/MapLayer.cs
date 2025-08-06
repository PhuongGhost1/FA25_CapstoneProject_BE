using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapLayer
{
    public int MapLayerId { get; set; }
    public Guid MapId { get; set; }
    public Guid LayerId { get; set; }
    public bool IsVisible { get; set; }
    public int ZIndex { get; set; }
    public int LayerOrder { get; set; }
    public string? CustomStyle { get; set; }
    public string? FilterConfig { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Map? Map { get; set; }
    public Layer? Layer { get; set; }
}
