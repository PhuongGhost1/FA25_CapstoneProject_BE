using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Users;

public class UserPreference
{
    public int UserPreferenceId { get; set; }
    public required Guid UserId { get; set; }
    public string Language { get; set; } = "en";
    public string DefaultMapStyle { get; set; } = "default";
    public string MeasurementUnit { get; set; } = "metric";
}
