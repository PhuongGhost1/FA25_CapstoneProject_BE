using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Services.OSM;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Osm;

public sealed class OsmEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Osm)
            .WithTags(Tags.Osm)
            .WithDescription("OpenStreetMap search & geocoding endpoints")
            .RequireAuthorization();

        MapSearch(group);
        MapReverseGeocode(group);
        MapGeocode(group);
        MapElementDetail(group);
    }

    private static void MapSearch(RouteGroupBuilder group)
    {
        group.MapGet(Routes.OsmEndpoints.Search, async (
                [AsParameters] OsmSearchQuery query,
                [FromServices] IOsmService osmService) =>
            {
                if (string.IsNullOrWhiteSpace(query.Query))
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Missing query",
                        Detail = "Parameter 'query' is required to perform an OpenStreetMap search."
                    });
                }

                var limit = Math.Clamp(query.Limit ?? 8, 1, 25);
                var sanitizedRadius = query.RadiusMeters is > 0 ? query.RadiusMeters : null;

                var results = await osmService.SearchByNameAsync(
                    query.Query.Trim(),
                    query.Lat,
                    query.Lon,
                    sanitizedRadius,
                    limit);

                return Results.Ok(results);
            })
            .WithName("SearchOsm")
            .WithDescription("Search OpenStreetMap via Nominatim and return lightweight feature records")
            .WithTags(Tags.Osm);
    }

    private static void MapReverseGeocode(RouteGroupBuilder group)
    {
        group.MapGet(Routes.OsmEndpoints.ReverseGeocode, async (
                [FromQuery(Name = "lat")] double lat,
                [FromQuery(Name = "lon")] double lon,
                [FromServices] IOsmService osmService) =>
            {
                var displayName = await osmService.GetReverseGeocodingAsync(lat, lon);
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Location not found",
                        Detail = "OSM reverse geocoding could not resolve the provided coordinates."
                    });
                }

                return Results.Ok(new { displayName });
            })
            .WithName("ReverseGeocodeOsm")
            .WithDescription("Resolve coordinates to the closest OpenStreetMap label")
            .WithTags(Tags.Osm);
    }

    private static void MapGeocode(RouteGroupBuilder group)
    {
        group.MapGet(Routes.OsmEndpoints.Geocode, async (
                [FromQuery] string address,
                [FromServices] IOsmService osmService) =>
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Missing address",
                        Detail = "Parameter 'address' is required for geocoding."
                    });
                }

                var result = await osmService.GeocodeAddressAsync(address.Trim());
                if (result is null)
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Address not found",
                        Detail = "OpenStreetMap did not return a match for the specified address."
                    });
                }

                return Results.Ok(result);
            })
            .WithName("GeocodeAddressOsm")
            .WithDescription("Forward geocode an address using OpenStreetMap")
            .WithTags(Tags.Osm);
    }

    private static void MapElementDetail(RouteGroupBuilder group)
    {
        group.MapGet(Routes.OsmEndpoints.ElementDetail, async (
                [FromRoute] string osmType,
                [FromRoute] long osmId,
                [FromServices] IOsmService osmService) =>
            {
                if (string.IsNullOrWhiteSpace(osmType))
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Missing element type",
                        Detail = "Route parameter 'osmType' must be provided."
                    });
                }

                var element = await osmService.GetElementByIdAsync(osmType.Trim().ToLowerInvariant(), osmId);
                if (element is null)
                {
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "OSM element not found",
                        Detail = $"OpenStreetMap element {osmType}/{osmId} could not be retrieved."
                    });
                }

                return Results.Ok(element);
            })
            .WithName("GetOsmElement")
            .WithDescription("Retrieve full element details (nodes/ways/relations) from Overpass")
            .WithTags(Tags.Osm);
    }

    private sealed record OsmSearchQuery(
        [property: FromQuery(Name = "query")] string Query,
        [property: FromQuery(Name = "lat")] double? Lat,
        [property: FromQuery(Name = "lon")] double? Lon,
        [property: FromQuery(Name = "radiusMeters")] double? RadiusMeters,
        [property: FromQuery(Name = "limit")] int? Limit);
}
