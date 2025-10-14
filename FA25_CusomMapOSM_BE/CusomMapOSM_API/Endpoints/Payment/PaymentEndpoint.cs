using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Payment;

public class PaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payment")
            .WithTags("Payment")
            .WithDescription("Payment processing endpoints for subscriptions and add-ons")
            .RequireAuthorization();

        // Subscribe to a plan
        group.MapPost("/subscribe", async (
                ClaimsPrincipal user,
                [FromBody] SubscribeRequest request,
                [FromServices] ISubscriptionService subscriptionService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                // Override userId from token for security
                request = request with { UserId = userId };

                var result = await subscriptionService.SubscribeToPlanAsync(request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("SubscribeToPlan")
            .WithDescription("Subscribe to a membership plan")
            .Produces<SubscribeResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(500);

        // Upgrade to a different plan
        group.MapPost("/upgrade", async (
                ClaimsPrincipal user,
                [FromBody] UpgradeRequest request,
                [FromServices] ISubscriptionService subscriptionService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                // Override userId from token for security
                request = request with { UserId = userId };

                var result = await subscriptionService.UpgradePlanAsync(request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpgradePlan")
            .WithDescription("Upgrade to a different membership plan")
            .Produces<UpgradeResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(500);


        // Confirm payment and process membership (webhook/callback)
        group.MapPost("/confirm", async (
                [FromBody] PaymentConfirmationRequest request,
                [FromServices] ITransactionService transactionService,
                [FromServices] ISubscriptionService subscriptionService,
                CancellationToken ct) =>
            {
                // First confirm the payment with transaction service
                if (!Guid.TryParse(request.TransactionId, out var transactionId))
                {
                    return Results.BadRequest("Invalid transaction ID");
                }

                var confirmRequest = new ConfirmPaymentWithContextReq
                {
                    PaymentGateway = request.PaymentGateway,    // payOS, stripe, vnpay
                    PaymentId = request.PaymentId,   // sessionId on processPayment
                    Purpose = request.Purpose,   // membership
                    TransactionId = transactionId,   // transactionId on processPayment
                    OrderCode = request.OrderCode, // orderCode for payOS
                };

                var confirmResult = await transactionService.ConfirmPaymentWithContextAsync(confirmRequest, ct);
                if (!confirmResult.HasValue)
                {
                    return Results.BadRequest("Failed to confirm payment");
                }

                // If payment was successful, process membership updates
                if (request.Status == "success")
                {
                    var processResult = await subscriptionService.ProcessSuccessfulPaymentAsync(transactionId, ct);
                    return processResult.Match(
                        success => Results.Ok(success),
                        error => error.ToProblemDetailsResult()
                    );
                }

                return Results.Ok(new PaymentConfirmationResponse
                {
                    TransactionId = request.TransactionId,
                    Status = request.Status,
                    Message = "Payment confirmation processed",
                    MembershipUpdated = false,
                    NotificationSent = false
                });
            })
            .WithName("ConfirmPayment")
            .WithDescription("Confirm payment completion and process membership (webhook/callback)")
            .AllowAnonymous() // Payment gateways need to call this
            .Produces<PaymentConfirmationResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // Cancel payment
        group.MapPost("/cancel", async (
                [FromBody] CancelPaymentWithContextReq request,
                [FromServices] ITransactionService transactionService,
                CancellationToken ct) =>
            {
                var result = await transactionService.CancelPaymentWithContextAsync(request, ct);
                return result.Match(
                    success => Results.Ok(new { success = true, message = "Payment cancelled successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CancelPayment")
            .WithDescription("Cancel a pending payment")
            .Produces<object>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

        // Get payment history
        group.MapGet("/history", async (
                ClaimsPrincipal user,
                [FromServices] ISubscriptionService subscriptionService,
                CancellationToken ct,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await subscriptionService.GetPaymentHistoryAsync(userId, page, pageSize, ct);
                return result.Match(
                    success => Results.Ok(new
                    {
                        payments = success,
                        page,
                        pageSize,
                        totalCount = success.Count,
                        hasMore = success.Count == pageSize // Simple check if there might be more data
                    }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetPaymentHistory")
            .WithDescription("Get user payment history")
            .Produces<object>(200)
            .ProducesProblem(401)
            .ProducesProblem(500);
    }
}