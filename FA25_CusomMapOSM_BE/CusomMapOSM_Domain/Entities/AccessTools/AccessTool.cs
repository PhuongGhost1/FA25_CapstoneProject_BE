using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.AccessTools;

public class AccessTool
{
    public int AccessToolId { get; set; }
    public string AccessToolName { get; set; } = string.Empty;
    public string AccessToolDescription { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public bool RequiredMembership { get; set; } = true;
}
