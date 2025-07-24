using CusomMapOSM_API.Interfaces;

namespace CusomMapOSM_API.Endpoints;

public class TestEndpoint : IEndpoint
{
    private const string API_PREFIX = "test";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX)
            .WithTags(Tags.Test)
            .WithDescription(Tags.Test);

        group.MapGet("/", () => Results.Ok("Test endpoint is working!"))
            .WithName("TestEndpoint")
            .WithTags("Test")
            .Produces<string>(StatusCodes.Status200OK)
            .WithMetadata(new HttpMethodMetadata(new[] { HttpMethods.Get }))
            .WithSummary("Test Endpoint")
            .WithDescription("This endpoint is used to test the API functionality.");
    }
}
