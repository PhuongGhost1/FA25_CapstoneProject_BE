using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Services.OSM
{
    public class GeocodingResultDTO
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, string> Address { get; set; }
        public double? Importance { get; set; }
        public BoundingBoxDTO BoundingBox { get; set; }
    }

    public class BoundingBoxDTO
    {
        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }
    }
}
