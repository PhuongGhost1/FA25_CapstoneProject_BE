using System.Text.Json.Serialization;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    public class NominatimResultDTO
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }
        
        [JsonPropertyName("licence")]
        public string Licence { get; set; }
        
        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; }
        
        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }
        
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        
        [JsonPropertyName("lat")]
        public string LatString { get; set; }
        
        [JsonPropertyName("lon")]
        public string LonString { get; set; }
        
        // Computed properties for easy access
        [JsonIgnore]
        public double Lat => double.TryParse(LatString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0;
        
        [JsonIgnore]
        public double Lon => double.TryParse(LonString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0;
        
        [JsonPropertyName("class")]
        public string Class { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("importance")]
        public double Importance { get; set; }
        
        [JsonPropertyName("boundingbox")]
        public string[] BoundingBox { get; set; }  // ["minLat", "maxLat", "minLon", "maxLon"]
        
        [JsonPropertyName("geojson")]
        public NominatimGeoJsonDTO GeoJson { get; set; }
        
        [JsonPropertyName("place_rank")]
        public int? PlaceRank { get; set; }
        
        [JsonPropertyName("address")]
        public NominatimAddressDTO Address { get; set; }
    }

    public class NominatimAddressDTO
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
        
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }
    
    public class NominatimGeoJsonDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("coordinates")]
        public object Coordinates { get; set; }  // Can be array or nested arrays
    }
}
