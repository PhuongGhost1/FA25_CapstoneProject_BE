using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users.Enums;

namespace CusomMapOSM_Domain.Entities.Users;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public Guid RoleId { get; set; }
    public AccountStatusEnum AccountStatus { get; set; } = AccountStatusEnum.PendingVerification;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public int MonthlyTokenUsage { get; set; } = 0;
    public DateTime? LastTokenReset { get; set; }

    public UserRole? Role { get; set; }
}
