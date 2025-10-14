using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.SupportTicket;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.SupportTicket;

public class SupportTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/support-tickets")
            .WithTags("SupportTicket")
            .WithDescription("Support ticket management endpoints for registered users")
            .RequireAuthorization();

        // Get user's support tickets
        group.MapGet("/", GetUserSupportTickets)
            .WithName("GetUserSupportTickets")
            .WithSummary("Get user's support tickets")
            .WithDescription("Retrieve paginated list of support tickets for the authenticated user")
            .Produces<SupportTicketListResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(500);

        // Get support ticket details
        group.MapGet("/{ticketId:int}", GetSupportTicketDetails)
            .WithName("GetSupportTicketDetails")
            .WithSummary("Get support ticket details")
            .WithDescription("Retrieve detailed information about a specific support ticket")
            .Produces<SupportTicketDto>(200)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Create support ticket
        group.MapPost("/", CreateSupportTicket)
            .WithName("CreateSupportTicket")
            .WithSummary("Create support ticket")
            .WithDescription("Create a new support ticket")
            .Produces<CreateSupportTicketResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(500);

        // Update support ticket
        group.MapPut("/{ticketId:int}", UpdateSupportTicket)
            .WithName("UpdateSupportTicket")
            .WithSummary("Update support ticket")
            .WithDescription("Update an existing support ticket (only if not closed)")
            .Produces<UpdateSupportTicketResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Close support ticket
        group.MapPost("/{ticketId:int}/close", CloseSupportTicket)
            .WithName("CloseSupportTicket")
            .WithSummary("Close support ticket")
            .WithDescription("Close a support ticket")
            .Produces<object>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Add message to support ticket
        group.MapPost("/{ticketId:int}/messages", AddTicketMessage)
            .WithName("AddTicketMessage")
            .WithSummary("Add message to support ticket")
            .WithDescription("Add a message to an existing support ticket")
            .Produces<AddTicketMessageResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Get support ticket messages
        group.MapGet("/{ticketId:int}/messages", GetTicketMessages)
            .WithName("GetTicketMessages")
            .WithSummary("Get support ticket messages")
            .WithDescription("Retrieve all messages for a specific support ticket")
            .Produces<List<SupportTicketMessageDto>>(200)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }

    private static async Task<IResult> GetUserSupportTickets(
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await supportTicketService.GetUserSupportTicketsAsync(userId.Value, page, pageSize, status, ct);
        return result.Match(
            success => Results.Ok(success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetSupportTicketDetails(
        int ticketId,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await supportTicketService.GetSupportTicketDetailsAsync(userId.Value, ticketId, ct);
        return result.Match(
            success => Results.Ok(success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> CreateSupportTicket(
        [FromBody] CreateSupportTicketRequest request,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await supportTicketService.CreateSupportTicketAsync(userId.Value, request, ct);
        return result.Match(
            success => Results.Created($"/api/support-tickets/{success.TicketId}", success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> UpdateSupportTicket(
        int ticketId,
        [FromBody] UpdateSupportTicketRequest request,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        // Ensure the ticketId in the request matches the route parameter
        request = request with { TicketId = ticketId };

        var result = await supportTicketService.UpdateSupportTicketAsync(userId.Value, request, ct);
        return result.Match(
            success => Results.Ok(success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> CloseSupportTicket(
        int ticketId,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await supportTicketService.CloseSupportTicketAsync(userId.Value, ticketId, ct);
        return result.Match(
            success => Results.Ok(new { success = true, message = "Support ticket closed successfully" }),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> AddTicketMessage(
        int ticketId,
        [FromBody] AddTicketMessageRequest request,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        // Ensure the ticketId in the request matches the route parameter
        request = request with { TicketId = ticketId };

        var result = await supportTicketService.AddTicketMessageAsync(userId.Value, request, ct);
        return result.Match(
            success => Results.Created($"/api/support-tickets/{ticketId}/messages/{success.MessageId}", success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static async Task<IResult> GetTicketMessages(
        int ticketId,
        ClaimsPrincipal user,
        ISupportTicketService supportTicketService,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await supportTicketService.GetTicketMessagesAsync(userId.Value, ticketId, ct);
        return result.Match(
            success => Results.Ok(success),
            error => error.ToProblemDetailsResult()
        );
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
