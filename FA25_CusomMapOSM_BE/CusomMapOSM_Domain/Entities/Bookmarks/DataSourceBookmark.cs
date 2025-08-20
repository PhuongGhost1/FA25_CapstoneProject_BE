using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
namespace CusomMapOSM_Domain.Entities.Bookmarks;

public class DataSourceBookmark
{
    public int DataSourceBookmarkId { get; set; }
    public required Guid UserId { get; set; }
    public required string OsmQuery { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? Creator { get; set; }
}
