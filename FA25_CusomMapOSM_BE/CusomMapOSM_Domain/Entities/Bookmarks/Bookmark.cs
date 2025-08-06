using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Bookmarks;

public class Bookmark
{
    public int BookmarkId { get; set; }
    public required Guid MapId { get; set; }
    public required Guid UserId { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? ViewState { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Map Map { get; set; } = new();
    public User User { get; set; } = new();
}
