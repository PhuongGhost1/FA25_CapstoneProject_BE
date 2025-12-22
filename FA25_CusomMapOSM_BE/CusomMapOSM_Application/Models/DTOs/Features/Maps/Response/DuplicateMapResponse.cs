using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class DuplicateMapResponse
    {
        public Guid MapId { get; set; }
        public string MapName { get; set; }
        public string? SourceMapName { get; set; }
        public int LayersCreated { get; set; }
        public int FeaturesCreated { get; set; }
        public int ImagesCreated { get; set; }
        public int SegmentsCreated { get; set; }
        public int LocationsCreated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = "Map duplicated successfully";
    }

    public class MoveMapToWorkspaceResponse
    {
        public Guid MapId { get; set; }
        public Guid? OldWorkspaceId { get; set; }
        public Guid NewWorkspaceId { get; set; }
        public string Message { get; set; } = "Map moved to workspace successfully";
    }
}
