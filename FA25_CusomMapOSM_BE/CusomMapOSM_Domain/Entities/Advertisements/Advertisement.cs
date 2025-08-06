using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Advertisements;

public class Advertisement
{
    public int AdvertisementId { get; set; }
    public string AdvertisementTitle { get; set; } = string.Empty;
    public string AdvertisementContent { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
