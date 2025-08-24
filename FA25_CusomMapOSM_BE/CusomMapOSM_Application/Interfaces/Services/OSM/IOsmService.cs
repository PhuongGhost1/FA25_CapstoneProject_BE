using CusomMapOSM_Application.Models.DTOs.Services.OSM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CusomMapOSM_Application.Interfaces.Services.OSM
{
    public interface IOsmService
    {
        Task<IEnumerable<OsmElementDTO>> SearchByNameAsync(string query, double lat, double lon, double radius);
        Task<IEnumerable<OsmElementDTO>> GetElementsInBoundingBoxAsync(double minLat, double minLon, double maxLat, double maxLon, string[] elementTypes = null);
        Task<OsmElementDetailDTO> GetElementByIdAsync(string type, long id);
        Task<GeocodingResultDTO> GeocodeAddressAsync(string address);
        Task<string> GetReverseGeocodingAsync(double lat, double lon);
    }
}