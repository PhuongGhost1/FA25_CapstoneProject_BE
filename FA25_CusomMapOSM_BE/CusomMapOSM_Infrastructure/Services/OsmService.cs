using CusomMapOSM_Application.Interfaces.Services.OSM;
using CusomMapOSM_Application.Models.DTOs.Services.OSM;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Infrastructure.Services
{
    public class OsmService : IOsmService
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ILogger<OsmService> _logger;
        private const string OVERPASS_API_URL = "https://overpass-api.de/api/interpreter";
        private const string NOMINATIM_API_URL = "https://nominatim.openstreetmap.org";
        private const int CACHE_MINUTES = 60;

        public OsmService(
            HttpClient httpClient,
            IDistributedCache cache,
            ILogger<OsmService> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CustomMapOSM_Application/1.0");
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<OsmElementDTO>> SearchByNameAsync(string query, double lat, double lon, double radius)
        {
            var cacheKey = $"osm:search:{query}:{lat}:{lon}:{radius}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonConvert.DeserializeObject<List<OsmElementDTO>>(cachedResult);
            }

            try
            {
                var nominatimUrl = $"{NOMINATIM_API_URL}/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1" +
                                  $"&limit=10&lat={lat}&lon={lon}&radius={radius}";
                
                var response = await _httpClient.GetAsync(nominatimUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<NominatimResultDTO>>(content);
                
                var osmElements = MapNominatimToOsmElements(results);
                
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(osmElements),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                    });
                
                return osmElements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OSM elements by name");
                return new List<OsmElementDTO>();
            }
        }

        public async Task<IEnumerable<OsmElementDTO>> GetElementsInBoundingBoxAsync(
            double minLat, double minLon, double maxLat, double maxLon, string[] elementTypes = null)
        {
            var cacheKey = $"osm:bbox:{minLat}:{minLon}:{maxLat}:{maxLon}:{string.Join(",", elementTypes ?? new string[0])}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonConvert.DeserializeObject<List<OsmElementDTO>>(cachedResult);
            }

            try
            {
                var elementFilters = string.Empty;
                if (elementTypes != null && elementTypes.Length > 0)
                {
                    elementFilters = $"[{string.Join("][", elementTypes)}]";
                }

                var query = $@"
                [out:json];
                (
                  node{elementFilters}({minLat},{minLon},{maxLat},{maxLon});
                  way{elementFilters}({minLat},{minLon},{maxLat},{maxLon});
                  relation{elementFilters}({minLat},{minLon},{maxLat},{maxLon});
                );
                out body;
                >;
                out skel qt;";

                var content = new StringContent(query, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(OVERPASS_API_URL, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var overpassResult = JsonConvert.DeserializeObject<OverpassResultDTO>(responseContent);
                
                var osmElements = MapOverpassToOsmElements(overpassResult);
                
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(osmElements),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                    });
                
                return osmElements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OSM elements in bounding box");
                return new List<OsmElementDTO>();
            }
        }

        public async Task<OsmElementDetailDTO> GetElementByIdAsync(string type, long id)
        {
            var cacheKey = $"osm:element:{type}:{id}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonConvert.DeserializeObject<OsmElementDetailDTO>(cachedResult);
            }

            try
            {
                // Prepare a query to get the element with full details
                var query = $@"
                [out:json];
                // Fetch the main element
                {type}({id});
                out body;
                // For ways, fetch all nodes
                {(type == "way" ? ">;out body;" : "")}
                // For relations, fetch all members
                {(type == "relation" ? ">;out body;" : "")}";

                var content = new StringContent(query, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(OVERPASS_API_URL, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var overpassResult = JsonConvert.DeserializeObject<OverpassResultDTO>(responseContent);
                
                // Convert to OsmElementDetailDTO
                var elementDetail = new OsmElementDetailDTO
                {
                    Id = id.ToString(),
                    Type = type
                };
                
                if (overpassResult?.Elements != null && overpassResult.Elements.Count > 0)
                {
                    // Find the main element
                    var mainElement = overpassResult.Elements.Find(e => e.Id == id && e.Type == type);
                    if (mainElement != null)
                    {
                        elementDetail.Tags = mainElement.Tags ?? new Dictionary<string, string>();
                        elementDetail.Lat = mainElement.Lat;
                        elementDetail.Lon = mainElement.Lon;
                        
                        // For ways, get all nodes
                        if (type == "way" && mainElement.Nodes != null)
                        {
                            foreach (var nodeId in mainElement.Nodes)
                            {
                                var node = overpassResult.Elements.Find(e => e.Id == nodeId && e.Type == "node");
                                if (node != null)
                                {
                                    elementDetail.Nodes.Add(new OsmNodeDTO
                                    {
                                        Id = node.Id,
                                        Lat = node.Lat ?? 0,
                                        Lon = node.Lon ?? 0,
                                        Tags = node.Tags ?? new Dictionary<string, string>()
                                    });
                                }
                            }
                        }
                        
                        // For relations, get all members
                        if (type == "relation" && mainElement.Members != null)
                        {
                            foreach (var member in mainElement.Members)
                            {
                                elementDetail.Members.Add(new OsmMemberDTO
                                {
                                    Type = member.Type,
                                    Ref = member.Ref,
                                    Role = member.Role
                                });
                            }
                        }
                    }
                }
                
                // Get additional details from Nominatim
                try
                {
                    var nominatimUrl = $"{NOMINATIM_API_URL}/lookup?osm_ids={type[0]}{id}&format=json&addressdetails=1";
                    var nominatimResponse = await _httpClient.GetAsync(nominatimUrl);
                    nominatimResponse.EnsureSuccessStatusCode();
                    
                    var nominatimContent = await nominatimResponse.Content.ReadAsStringAsync();
                    var nominatimResults = JsonConvert.DeserializeObject<List<NominatimResultDTO>>(nominatimContent);
                    
                    if (nominatimResults != null && nominatimResults.Count > 0)
                    {
                        elementDetail.DisplayName = nominatimResults[0].DisplayName;
                        elementDetail.Category = nominatimResults[0].Class;
                        
                        if (nominatimResults[0].Address != null)
                        {
                            elementDetail.Address = new Dictionary<string, string>
                            {
                                { "road", nominatimResults[0].Address.Road },
                                { "city", nominatimResults[0].Address.City },
                                { "state", nominatimResults[0].Address.State },
                                { "postcode", nominatimResults[0].Address.Postcode },
                                { "country", nominatimResults[0].Address.Country },
                                { "country_code", nominatimResults[0].Address.CountryCode }
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get additional details from Nominatim for {Type} {Id}", type, id);
                    // Continue without Nominatim data
                }
                
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(elementDetail),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                    });
                
                return elementDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OSM element details for {Type} {Id}", type, id);
                throw;
            }
        }

        public async Task<GeocodingResultDTO> GeocodeAddressAsync(string address)
        {
            var cacheKey = $"osm:geocode:{address}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonConvert.DeserializeObject<GeocodingResultDTO>(cachedResult);
            }

            try
            {
                var geocodeUrl = $"{NOMINATIM_API_URL}/search?q={Uri.EscapeDataString(address)}&format=json&addressdetails=1&limit=1";
                
                var response = await _httpClient.GetAsync(geocodeUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<NominatimResultDTO>>(content);
                
                if (results == null || results.Count == 0)
                {
                    _logger.LogWarning("No geocoding results found for address: {Address}", address);
                    return null;
                }
                
                var result = results[0];
                var geocodingResult = new GeocodingResultDTO
                {
                    Lat = result.Lat,
                    Lon = result.Lon,
                    DisplayName = result.DisplayName,
                    Importance = result.Importance,
                    Address = new Dictionary<string, string>()
                };
                
                if (result.Address != null)
                {
                    geocodingResult.Address = new Dictionary<string, string>
                    {
                        { "road", result.Address.Road },
                        { "city", result.Address.City },
                        { "state", result.Address.State },
                        { "postcode", result.Address.Postcode },
                        { "country", result.Address.Country },
                        { "country_code", result.Address.CountryCode }
                    };
                }
                
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(geocodingResult),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                    });
                
                return geocodingResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address);
                return null;
            }
        }

        public async Task<string> GetReverseGeocodingAsync(double lat, double lon)
        {
            var cacheKey = $"osm:revgeocode:{lat}:{lon}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return cachedResult;
            }

            try
            {
                var reverseGeocodeUrl = $"{NOMINATIM_API_URL}/reverse?lat={lat}&lon={lon}&format=json&addressdetails=1";
                
                var response = await _httpClient.GetAsync(reverseGeocodeUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<NominatimResultDTO>(content);
                
                if (result == null)
                {
                    _logger.LogWarning("No reverse geocoding results found for coordinates: {Lat}, {Lon}", lat, lon);
                    return null;
                }
                
                var displayName = result.DisplayName;
                
                await _cache.SetStringAsync(
                    cacheKey,
                    displayName,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                    });
                
                return displayName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverse geocoding coordinates: {Lat}, {Lon}", lat, lon);
                return null;
            }
        }

        // Implement remaining methods...

        private IEnumerable<OsmElementDTO> MapNominatimToOsmElements(List<NominatimResultDTO> results)
        {
            var elements = new List<OsmElementDTO>();
            
            foreach (var result in results)
            {
                elements.Add(new OsmElementDTO
                {
                    Id = result.OsmId.ToString(),
                    Type = result.OsmType,
                    Tags = new Dictionary<string, string>
                    {
                        { "name", result.DisplayName },
                        { "lat", result.Lat.ToString() },
                        { "lon", result.Lon.ToString() },
                        // Add other properties...
                    }
                });
            }
            
            return elements;
        }

        private IEnumerable<OsmElementDTO> MapOverpassToOsmElements(OverpassResultDTO result)
        {
            var elements = new List<OsmElementDTO>();
            
            if (result?.Elements != null)
            {
                foreach (var element in result.Elements)
                {
                    elements.Add(new OsmElementDTO
                    {
                        Id = element.Id.ToString(),
                        Type = element.Type,
                        Tags = element.Tags ?? new Dictionary<string, string>(),
                        Lat = element.Lat,
                        Lon = element.Lon,
                        Nodes = element.Nodes
                    });
                }
            }
            
            return elements;
        }
    }
}