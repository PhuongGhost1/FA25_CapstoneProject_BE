using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Domain.Entities.Annotations;

public class Annotation
{
    public int AnnotationId { get; set; }
    public required Guid TypeId { get; set; }
    public required Guid MapId { get; set; }
    public string? Geometry { get; set; } // Use spatial type in DB, string/json here
    public string? Properties { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AnnotationType Type { get; set; } = new();
    public Map Map { get; set; } = new();
}
