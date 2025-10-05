using System;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Organizations.Enums;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class Organization
{
    public Guid OrgId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public Guid OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public OrganizationStatusEnum Status { get; set; } = OrganizationStatusEnum.Active;

    public User? Owner { get; set; }
}
