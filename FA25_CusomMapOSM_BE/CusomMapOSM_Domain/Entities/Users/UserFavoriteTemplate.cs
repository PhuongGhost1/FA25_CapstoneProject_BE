using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Domain.Entities.Users;

public class UserFavoriteTemplate
{
    public int UserFavoriteTemplateId { get; set; }
    public required Guid UserId { get; set; }
    public required int TemplateId { get; set; }
    public DateTime FavoriteAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = new();
    public MapTemplate Template { get; set; } = new();
}
