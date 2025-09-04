using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    public class OsmElementDTO
    {
        public string Id { get; set; }
        public string Type { get; set; }  // node, way, relation
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public List<long>? Nodes { get; set; }  // For ways
        public Dictionary<string, List<long>>? Members { get; set; }  // For relations
    }
}
