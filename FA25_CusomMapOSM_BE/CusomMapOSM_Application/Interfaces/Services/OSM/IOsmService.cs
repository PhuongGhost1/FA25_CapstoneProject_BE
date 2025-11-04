using CusomMapOSM_Application.Models.DTOs.Services.OSM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CusomMapOSM_Application.Interfaces.Services.OSM
{
    public interface IOsmService
    {
        Task<IEnumerable<OsmSearchResultDTO>> SearchByNameAsync(string query, double? lat = null, double? lon = null, double? radiusMeters = null, int limit = 10);
        Task<IEnumerable<OsmElementDTO>> GetElementsInBoundingBoxAsync(double minLat, double minLon, double maxLat, double maxLon, string[] elementTypes = null);
        Task<OsmElementDetailDTO> GetElementByIdAsync(string type, long id);
        Task<GeocodingResultDTO> GeocodeAddressAsync(string address);
        Task<string> GetReverseGeocodingAsync(double lat, double lon);
    }
}
