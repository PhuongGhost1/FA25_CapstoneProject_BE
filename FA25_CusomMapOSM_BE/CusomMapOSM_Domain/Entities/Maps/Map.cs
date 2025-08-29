using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

public class Map
{
    public Guid MapId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrgId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImage { get; set; }
    public bool IsTemplate { get; set; } = false;                  
    public Guid? ParentMapId { get; set; }                     
    public MapTemplateCategoryEnum? Category { get; set; }         
    public bool IsFeatured { get; set; } = false;               
    public int UsageCount { get; set; } = 0;                      
    public string? DefaultBounds { get; set; }                      
    public string? MapConfig { get; set; }
    public string BaseLayer { get; set; } = "osm";
    public string? ViewState { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int TotalLayers { get; set; }  
    public int TotalFeatures { get; set; }  
    public bool IsClonedFromTemplate => ParentMapId.HasValue;
    public User? User { get; set; }
    public Organization? Organization { get; set; }
    public Map? ParentMap { get; set; }                          
    public ICollection<Map>? ClonedMaps { get; set; }                   
    public ICollection<MapLayer>? MapLayers { get; set; }
    public ICollection<MapAnnotation>? MapAnnotations { get; set; }
    public ICollection<MapImage>? MapImages { get; set; }
}
