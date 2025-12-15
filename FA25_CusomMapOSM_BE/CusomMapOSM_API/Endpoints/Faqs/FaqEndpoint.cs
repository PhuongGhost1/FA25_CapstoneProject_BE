using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Faqs;
using CusomMapOSM_Application.Models.DTOs.Features.Faqs;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Faqs;

public class FaqEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/faqs")
            .WithTags("FAQs")
            .WithDescription("Frequently Asked Questions endpoints");

        // Get all FAQs - Public endpoint (no authentication required)
        group.MapGet("/", async (
                [FromServices] IFaqService faqService,
                CancellationToken ct) =>
            {
                var result = await faqService.GetAllFaqsAsync(ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetAllFaqs")
            .WithDescription("Get all frequently asked questions")
            .Produces<GetAllFaqsResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // Get FAQs by category - Public endpoint
        group.MapGet("/category/{category}", async (
                [FromRoute] string category,
                [FromServices] IFaqService faqService,
                CancellationToken ct) =>
            {
                var result = await faqService.GetFaqsByCategoryAsync(category, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetFaqsByCategory")
            .WithDescription("Get FAQs by category")
            .Produces<GetFaqsByCategoryResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get FAQ by ID - Public endpoint
        group.MapGet("/{faqId:int}", async (
                [FromRoute] int faqId,
                [FromServices] IFaqService faqService,
                CancellationToken ct) =>
            {
                var result = await faqService.GetFaqByIdAsync(faqId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetFaqById")
            .WithDescription("Get FAQ by ID")
            .Produces<GetFaqByIdResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get FAQ categories - Public endpoint
        group.MapGet("/categories", async (
                [FromServices] IFaqService faqService,
                CancellationToken ct) =>
            {
                var result = await faqService.GetFaqCategoriesAsync(ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetFaqCategories")
            .WithDescription("Get all FAQ categories")
            .Produces<GetFaqCategoriesResponse>(200)
            .ProducesProblem(500);
    }
}
