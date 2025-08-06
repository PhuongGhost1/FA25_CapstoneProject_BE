using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Comments;

public class Comment
{
    public int CommentId { get; set; }
    public Guid? MapId { get; set; }
    public Guid? LayerId { get; set; }
    public Guid? UserId { get; set; }
    public required string Content { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    public Map? Map { get; set; }
    public Layer? Layer { get; set; }
    public User? User { get; set; }
}
