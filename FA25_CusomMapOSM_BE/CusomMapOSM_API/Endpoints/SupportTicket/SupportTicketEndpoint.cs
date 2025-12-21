using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.SupportTicket;
using CusomMapOSM_Application.Models.DTOs.Features.SupportTicket.Request;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.SupportTicket;

public class SupportTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/support-tickets")
            .WithTags("SupportTicket")
            .WithDescription("Support ticket management endpoints for registered users")
            .RequireAuthorization();

        // User create support ticket
        group.MapPost("/",
                async ([FromServices] ISupportTicketService service, [FromBody] CreateSupportTicketRequest request) =>
                {
                    var result = await service.CreateSupportTicket(request);
                    return result.Match(
                        success => Results.Ok(success),
                        error => error.ToProblemDetailsResult()
                    );
                })
            .WithName("CreateSupportTicket")
            .WithSummary("Create a new support ticket")
            .WithDescription("Create a new support ticket for the current user")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //User get support tickets
        group.MapGet("/", async ([FromServices] ISupportTicketService service, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            {
                var result = await service.GetSupportTickets(page, pageSize);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.BadRequest(new { error = error.Description })
                );
            })
            .WithName("GetSupportTickets")
            .WithSummary("Get all support tickets for the current user")
            .WithDescription("Get all support tickets for the current user")            
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //User get support ticket by id
        group.MapGet("/{ticketId}", async ([FromServices] ISupportTicketService service, [FromRoute] int ticketId) =>
            {
                var result = await service.GetSupportTicketById(ticketId);
                    return result.Match(
                    success => Results.Ok(success),
                    error => Results.NotFound(new { error = error.Description })
                );
            })
            .WithName("GetSupportTicketById")
            .WithSummary("Get a support ticket by id")
            .WithDescription("Get a support ticket by id")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //User response to support ticket
        group.MapPost("/{ticketId}/response", async ([FromServices] ISupportTicketService service,
                [FromRoute] int ticketId, [FromBody] ResponseSupportTicketRequest request) =>
            {
                var result = await service.ResponseToSupportTicket(ticketId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.NotFound(new { error = error.Description })
                );
            })
            .WithName("ResponseToSupportTicket")
            .WithSummary("Response to a support ticket")
            .WithDescription("Response to a support ticket")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //Admin get support tickets
        group.MapGet("/admin", async ([FromServices] ISupportTicketService service, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            {
                var result = await service.GetAllSupportTicketsForAdmin(page, pageSize);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.BadRequest(new { error = error.Description })
                );
            })
            .WithName("GetSupportTicketsByAdmin")
            .WithSummary("Get all support tickets for the admin")
            .WithDescription("Get all support tickets for the admin")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //Admin get support ticket by id
        group.MapGet("/admin/{ticketId}",
                async ([FromServices] ISupportTicketService service, [FromRoute] int ticketId) =>
                {
                    var result = await service.GetSupportTicketByIdForAdmin(ticketId);
                    return result.Match(
                    success => Results.Ok(success),
                    error => Results.NotFound(new { error = error.Description })
                );
                })
            .WithName("GetSupportTicketByIdByAdmin")
            .WithSummary("Get a support ticket by id for the admin")
            .WithDescription("Get a support ticket by id for the admin")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //Admin reply to support ticket
        group.MapPost("/admin/{ticketId}/reply", async ([FromServices] ISupportTicketService service,
                [FromRoute] int ticketId, [FromBody] ReplySupportTicketRequest request) =>
            {
                var result = await service.ReplyToSupportTicket(ticketId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.NotFound(new { error = error.Description })
                );
            })
            .WithName("ReplyToSupportTicketByAdmin")
            .WithSummary("Reply to a support ticket for the admin")
            .WithDescription("Reply to a support ticket for the admin")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        //Admin close support ticket
        group.MapPost("/admin/{ticketId}/close",
                async ([FromServices] ISupportTicketService service, [FromRoute] int ticketId) =>
                {
                    var result = await service.CloseSupportTicket(ticketId);
                    return result.Match(
                    success => Results.Ok(success),
                    error => Results.NotFound(new { error = error.Description })
                );
                })
            .WithName("CloseSupportTicketByAdmin")
            .WithSummary("Close a support ticket for the admin")
            .WithDescription("Close a support ticket for the admin")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);
    }
}