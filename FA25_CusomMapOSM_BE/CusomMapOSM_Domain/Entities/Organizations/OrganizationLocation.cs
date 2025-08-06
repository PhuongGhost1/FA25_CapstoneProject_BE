using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class OrganizationLocation
{
    public int LocationId { get; set; }
    public Guid OrgId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? OperatingHours { get; set; }
    public string? Services { get; set; }
    public string? Categories { get; set; }
    public string? Amenities { get; set; }
    public string? Photos { get; set; }
    public string? SocialMedia { get; set; }
    public Guid OrganizationLocationsStatusId { get; set; }
    public bool Verified { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
    public OrganizationLocationStatus? Status { get; set; }
}
