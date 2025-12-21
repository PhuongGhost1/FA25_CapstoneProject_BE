using CusomMapOSM_Application.Models.DTOs.Services.OSM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CusomMapOSM_Application.Interfaces.Services.OSM
{
    public interface IOsmService
    {
        Task<IEnumerable<OsmSearchResultDTO>> SearchByNameAsync(string? name = null, string? city = null, string? state = null, string? country = null, double? lat = null, double? lon = null, double? radiusMeters = null, int limit = 10);
        Task<IEnumerable<OsmElementDTO>> GetElementsInBoundingBoxAsync(double minLat, double minLon, double maxLat, double maxLon, string[]? elementTypes = null);
        Task<OsmElementDetailDTO> GetElementByIdAsync(string type, long id);
        Task<GeocodingResultDTO> GeocodeAddressAsync(string address);
        Task<string> GetReverseGeocodingAsync(double lat, double lon);
        Task<string> GetRouteBetweenPointsAsync(double fromLat, double fromLon, double toLat, double toLon, string routeType = "road");
        Task<string> GetRouteWithWaypointsAsync(List<(double lat, double lon)> waypoints, string routeType = "road");
    }
}
