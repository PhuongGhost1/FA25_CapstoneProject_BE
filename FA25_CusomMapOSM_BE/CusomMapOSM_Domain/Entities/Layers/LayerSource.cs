using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Layers;

public class LayerSource
{
    public Guid SourceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}
