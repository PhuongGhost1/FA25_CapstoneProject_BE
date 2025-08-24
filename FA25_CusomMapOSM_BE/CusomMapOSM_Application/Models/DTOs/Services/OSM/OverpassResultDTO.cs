using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    public class OverpassResultDTO
    {
        [JsonPropertyName("version")]
        public double Version { get; set; }
        
        [JsonPropertyName("generator")]
        public string Generator { get; set; }
        
        [JsonPropertyName("elements")]
        public List<OverpassElementDTO> Elements { get; set; }
    }

    public class OverpassElementDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }  // node, way, relation
        
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("lat")]
        public double? Lat { get; set; }
        
        [JsonPropertyName("lon")]
        public double? Lon { get; set; }
        
        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; }
        
        [JsonPropertyName("nodes")]
        public List<long> Nodes { get; set; }  // For ways
        
        [JsonPropertyName("members")]
        public List<OverpassMemberDTO> Members { get; set; }  // For relations
    }

    public class OverpassMemberDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("ref")]
        public long Ref { get; set; }
        
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
