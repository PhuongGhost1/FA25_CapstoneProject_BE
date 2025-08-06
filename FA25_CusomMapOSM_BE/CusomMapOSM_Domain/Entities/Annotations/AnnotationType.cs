using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Annotations;

public class AnnotationType
{
    public Guid TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
