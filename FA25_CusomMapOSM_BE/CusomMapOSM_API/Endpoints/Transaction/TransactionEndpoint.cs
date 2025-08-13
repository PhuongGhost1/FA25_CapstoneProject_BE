using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Transaction;

public class TransactionEndpoint : IEndpoint
{
    private const string API_PREFIX = "transaction";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX).RequireAuthorization();

        group.MapPost("/process-payment", async (
            ITransactionService factory,
            ProcessPaymentReq request,
            CancellationToken ct) =>
        {
            if (request.Total <= 0)
                return Results.BadRequest("Invalid payment amount");

            var result = await factory.ProcessPaymentAsync(request, ct);

            return result.Match<IResult>(
                some: approval => Results.Ok(approval),
                none: err => err.ToProblemDetailsResult()
            );
        });

        group.MapPost("/confirm-payment-with-context", async (
            ITransactionService factory,
            ConfirmPaymentWithContextReq req,
            CancellationToken ct) =>
        {
            var result = await factory.ConfirmPaymentWithContextAsync(req, ct);

            return result.Match<IResult>(
                some: ok => Results.Ok(ok),
                none: err => err.ToProblemDetailsResult()
            );
        });

        group.MapPost("/cancel-payment", async (
            ITransactionService factory,
            CancelPaymentWithContextReq req,
            CancellationToken ct) =>
        {
            var result = await factory.CancelPaymentWithContextAsync(req, ct);
            return result.Match<IResult>(
                some: ok => Results.Ok(ok),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("CancelPayment")
        .WithDescription("Cancel a payment transaction")
        .WithTags("Transaction");

        group.MapGet("/{transactionId:guid}", async (ITransactionService txService, Guid transactionId, CancellationToken ct) =>
        {
            var result = await txService.GetTransactionAsync(transactionId, ct);
            return result.Match<IResult>(
                some: transaction => Results.Ok(transaction),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("GetTransaction")
        .WithDescription("Get transaction details by ID")
        .WithTags("Transaction");
    }
}