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
        public double Lat { get; set; }
        
        [JsonPropertyName("lon")]
        public double Lon { get; set; }
        
        [JsonPropertyName("class")]
        public string Class { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("importance")]
        public double Importance { get; set; }
        
        [JsonPropertyName("address")]
        public NominatimAddressDTO Address { get; set; }
    }

    public class NominatimAddressDTO
    {
        [JsonPropertyName("road")]
        public string Road { get; set; }
        
        [JsonPropertyName("city")]
        public string City { get; set; }
        
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }
        
        [JsonPropertyName("country")]
        public string Country { get; set; }
        
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }
}
