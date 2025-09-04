using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    public class OsmElementDetailDTO
    {
        public string Id { get; set; }
        public string Type { get; set; }  // node, way, relation
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public List<OsmNodeDTO> Nodes { get; set; } = new List<OsmNodeDTO>();  // For ways
        public List<OsmMemberDTO> Members { get; set; } = new List<OsmMemberDTO>();  // For relations
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> Address { get; set; } = new Dictionary<string, string>();
    }

    public class OsmNodeDTO
    {
        public long Id { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class OsmMemberDTO
    {
        public string Type { get; set; }  // node, way, relation
        public long Ref { get; set; }
        public string Role { get; set; }
    }
}
