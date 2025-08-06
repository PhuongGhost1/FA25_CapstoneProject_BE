using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

public class Map
{
    public Guid MapId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GeographicBounds { get; set; }
    public string? MapConfig { get; set; }
    public string BaseLayer { get; set; } = "osm";
    public string? ViewState { get; set; }
    public string? PreviewImage { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Organization? Organization { get; set; }
    public MapTemplate? Template { get; set; }
}
