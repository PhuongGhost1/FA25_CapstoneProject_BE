using System.Text.Json.Serialization;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    /// <summary>
    /// Lightweight DTO for OSM search results - optimized for map display and zone creation
    /// </summary>
    public class OsmSearchResultDTO
    {
        [JsonPropertyName("osmType")]
        public string? OsmType { get; set; }
        
        [JsonPropertyName("osmId")]
        public long OsmId { get; set; }
        
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
        
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        
        [JsonPropertyName("lon")]
        public double Lon { get; set; }
        
        [JsonPropertyName("boundingBox")]
        public double[]? BoundingBox { get; set; }  // [minLat, maxLat, minLon, maxLon]
        
        [JsonPropertyName("category")]
        public string? Category { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        [JsonPropertyName("importance")]
        public double? Importance { get; set; }
        
        // Additional fields for Zone creation
        [JsonPropertyName("geoJson")]
        public string? GeoJson { get; set; }  // GeoJSON geometry (Point for nodes, LineString/Polygon for ways/relations)
        
        [JsonPropertyName("addressDetails")]
        public OsmAddressDTO? AddressDetails { get; set; }
        
        [JsonPropertyName("placeRank")]
        public int? PlaceRank { get; set; }  // Helps determine administrative level
        
        [JsonPropertyName("adminLevel")]
        public int? AdminLevel { get; set; }  // OSM admin_level tag
    }
    
    public class OsmAddressDTO
    {
        [JsonPropertyName("road")]
        public string Road { get; set; }
        
        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }
        
        [JsonPropertyName("city")]
        public string City { get; set; }
        
        [JsonPropertyName("district")]
        public string District { get; set; }
        
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }
        
        [JsonPropertyName("country")]
        public string Country { get; set; }
        
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }
    }
}
