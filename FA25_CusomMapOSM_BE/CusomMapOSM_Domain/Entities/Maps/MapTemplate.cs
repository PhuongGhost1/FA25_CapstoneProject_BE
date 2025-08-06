using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapTemplate
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImage { get; set; }
    public string? DefaultBounds { get; set; }
    public string? TemplateConfig { get; set; }
    public string BaseLayer { get; set; } = "osm";
    public string? InitialLayers { get; set; }
    public string? ViewState { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int UsageCount { get; set; } = 0;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? Creator { get; set; }
}
