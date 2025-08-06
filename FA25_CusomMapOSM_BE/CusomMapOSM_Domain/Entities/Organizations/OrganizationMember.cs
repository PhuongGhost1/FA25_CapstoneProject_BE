using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Organizations;

public class OrganizationMember
{
    public Guid MemberId { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public Guid MembersRoleId { get; set; }
    public Guid? InvitedBy { get; set; }
    public DateTime? JoinedAt { get; set; }
    public bool IsActive { get; set; }

    public Organization? Organization { get; set; }
    public User? User { get; set; }
    public OrganizationMemberType? Role { get; set; }
    public User? Inviter { get; set; }
}
