using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Services.Payment;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Application.Common.Errors;

namespace CusomMapOSM_Infrastructure.Features.Transaction;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentService _paymentService;
    private readonly IMembershipService _membershipService;
    private readonly IUserAccessToolService _userAccessToolService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPaymentGatewayRepository _paymentGatewayRepository;
    public TransactionService(ITransactionRepository transactionRepository, IPaymentService paymentService, IMembershipService membershipService, IUserAccessToolService userAccessToolService, IServiceProvider serviceProvider, IPaymentGatewayRepository paymentGatewayRepository)
    {
        _transactionRepository = transactionRepository;
        _paymentService = paymentService;
        _membershipService = membershipService;
        _userAccessToolService = userAccessToolService;
        _serviceProvider = serviceProvider;
        _paymentGatewayRepository = paymentGatewayRepository;
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> ProcessPaymentAsync(ProcessPaymentReq request, CancellationToken ct)
    {
        // 1. Get Gateway ID
        var gatewayIdResult = await GetPaymentGatewayIdAsync(request.PaymentGateway, ct);
        if (!gatewayIdResult.HasValue)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorCustom.ErrorType.NotFound));

        var gatewayId = gatewayIdResult.ValueOr(default(Guid));

        // 2. Create pending transaction
        var pendingTransactionResult = await CreateTransactionRecordAsync(
            gatewayId,
            request.Total,
            request.Purpose,
            request.MembershipId,
            null,
            "pending",
            ct
        );

        if (!pendingTransactionResult.HasValue)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.Create.Failed", "Failed to create transaction", ErrorCustom.ErrorType.Failure));

        var pendingTransaction = pendingTransactionResult.ValueOr(default(Transactions));

        // 3. Get PaymentService
        var paymentService = GetPaymentService(request.PaymentGateway);

        // 4. Create checkout
        var checkoutResult = await paymentService.CreateCheckoutAsync(
            request.Total,
            $"http://localhost:5233/transaction/confirm-payment-with-context?transactionId={pendingTransaction.TransactionId}",
            $"http://localhost:5233/transaction/cancel-payment?transactionId={pendingTransaction.TransactionId}",
            ct
        );

        if (!checkoutResult.HasValue)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Checkout.Failed", "Failed to create checkout", ErrorCustom.ErrorType.Failure));

        var approval = checkoutResult.ValueOr(default(ApprovalUrlResponse));

        // 5. Save gateway session/payment ID
        await UpdateTransactionGatewayInfoAsync(
            pendingTransaction.TransactionId,
            approval.SessionId,
            ct
        );

        return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(approval);
    }

    public IPaymentService GetPaymentService(PaymentGatewayEnum gateway)
    {
        return gateway switch
        {
            PaymentGatewayEnum.PayOS => _paymentService, // Use the injected payment service directly
            _ => throw new ArgumentException("Invalid payment gateway")
        };
    }

    public async Task<Option<object, ErrorCustom.Error>> ConfirmPaymentWithContextAsync(
        ConfirmPaymentWithContextReq req,
        CancellationToken ct)
    {
        // 1. If we already have a TransactionId, reuse it
        Transactions transaction;

        if (req.TransactionId != Guid.Empty)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(req.TransactionId, ct);
            if (existingTransaction is null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound));

            transaction = existingTransaction;
        }
        else
        {
            // Fallback: Create a new transaction if none exists
            var gatewayIdResult = await GetPaymentGatewayIdAsync(req.PaymentGateway, ct);
            if (!gatewayIdResult.HasValue)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorCustom.ErrorType.NotFound));

            var gatewayId = gatewayIdResult.ValueOr(default(Guid));

            var initialTransactionResult = await CreateTransactionRecordAsync(
                gatewayId,
                0,
                req.Purpose,
                req.MembershipId,
                null,
                "pending",
                ct
            );

            if (!initialTransactionResult.HasValue)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Transaction.Create.Failed", "Failed to create transaction", ErrorCustom.ErrorType.Failure));

            transaction = initialTransactionResult.ValueOr(default(Transactions));
        }

        // 2. Get correct payment service
        var paymentService = GetPaymentService(req.PaymentGateway);

        // 3. Confirm payment
        var confirmed = await paymentService.ConfirmPaymentAsync(new ConfirmPaymentReq
        {
            PaymentGateway = req.PaymentGateway,
            PaymentId = req.PaymentId,
            PayerId = req.PayerId,
            Token = req.Token,
            PaymentIntentId = req.PaymentIntentId,
            ClientSecret = req.ClientSecret,
            OrderCode = req.OrderCode,
            Signature = req.Signature
        }, ct);

        return await confirmed.Match(
            some: async _ =>
            {
                await UpdateTransactionStatusAsync(transaction.TransactionId, "success", ct);

                // 4. Post-payment fulfillment logic
                return await HandlePostPaymentAsync(req, transaction.TransactionId, ct);
            },
            none: async err =>
            {
                await UpdateTransactionStatusAsync(transaction.TransactionId, "failed", ct);
                return Option.None<object, ErrorCustom.Error>(err);
            }
        );
    }

    private async Task<Option<object, ErrorCustom.Error>> HandlePostPaymentAsync(ConfirmPaymentWithContextReq req, Guid transactionId, CancellationToken ct)
    {
        if (string.Equals(req.Purpose, "membership", StringComparison.OrdinalIgnoreCase))
        {
            if (req.UserId is null || req.OrgId is null || req.PlanId is null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Context.Invalid", "Missing membership context", ErrorCustom.ErrorType.Validation));

            var membership = await _membershipService.CreateOrRenewMembershipAsync(
                req.UserId.Value,
                req.OrgId.Value,
                req.PlanId.Value,
                autoRenew: true,
                ct
            );

            return await membership.Match(
                some: async m =>
                {
                    // Grant access tools based on the membership plan
                    var accessToolResult = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(
                        req.UserId.Value,
                        req.PlanId.Value,
                        m.EndDate ?? DateTime.UtcNow.AddMonths(1), // Default to 1 month if no end date
                        ct
                    );

                    accessToolResult.Match(
                        some: _ => { /* Success - do nothing */ },
                        none: error => Console.WriteLine($"Failed to grant access tools: {error?.Description ?? "Unknown error"}")
                    );

                    return Option.Some<object, ErrorCustom.Error>(new
                    {
                        MembershipId = m.MembershipId,
                        TransactionId = transactionId,
                        AccessToolsGranted = accessToolResult.HasValue
                    });
                },
                none: err => Task.FromResult(Option.None<object, ErrorCustom.Error>(err))
            );
        }

        if (string.Equals(req.Purpose, "addon", StringComparison.OrdinalIgnoreCase))
        {
            if (req.MembershipId is null || req.OrgId is null || string.IsNullOrWhiteSpace(req.AddonKey))
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Context.Invalid", "Missing addon context", ErrorCustom.ErrorType.Validation));

            var addon = await _membershipService.AddAddonAsync(
                req.MembershipId.Value,
                req.OrgId.Value,
                req.AddonKey,
                req.Quantity,
                true,
                ct
            );

            return addon.Match(
                some: a => Option.Some<object, ErrorCustom.Error>(new
                {
                    AddonId = a.AddonId,
                    TransactionId = transactionId
                }),
                none: err => Option.None<object, ErrorCustom.Error>(err)
            );
        }

        return Option.Some<object, ErrorCustom.Error>(new
        {
            Status = "ok",
            TransactionId = transactionId
        });
    }

    public async Task<Option<Guid, ErrorCustom.Error>> GetPaymentGatewayIdAsync(PaymentGatewayEnum paymentGateway, CancellationToken ct)
    {
        var gateway = await _paymentGatewayRepository.GetByIdAsync(paymentGateway, ct);
        if (gateway == null)
            return Option.None<Guid, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorCustom.ErrorType.NotFound));

        return Option.Some<Guid, ErrorCustom.Error>(gateway.GatewayId);
    }

    public async Task<Option<Transactions, ErrorCustom.Error>> CreateTransactionRecordAsync(Guid paymentGatewayId, decimal amount, string purpose, Guid? membershipId, int? exportId, string status, CancellationToken ct)
    {
        var tx = new Transactions
        {
            TransactionId = Guid.NewGuid(),
            PaymentGatewayId = paymentGatewayId,
            Amount = amount,
            Status = status,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            MembershipId = membershipId,
            ExportId = exportId,
            Purpose = purpose,
            TransactionReference = Guid.NewGuid().ToString("N")
        };

        var result = await _transactionRepository.CreateAsync(tx, ct);
        return Option.Some<Transactions, ErrorCustom.Error>(result);
    }

    public async Task<Option<Transactions, ErrorCustom.Error>> UpdateTransactionStatusAsync(Guid transactionId, string status, CancellationToken ct)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
        if (transaction == null)
            return Option.None<Transactions, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound));

        transaction.Status = status;
        var result = await _transactionRepository.UpdateAsync(transaction, ct);
        if (result is null)
            return Option.None<Transactions, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.Update.Failed", "Failed to update transaction status", ErrorCustom.ErrorType.Failure));

        return Option.Some<Transactions, ErrorCustom.Error>(result);
    }

    public async Task<Option<Transactions, ErrorCustom.Error>> GetTransactionAsync(Guid transactionId, CancellationToken ct)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
        return transaction != null
            ? Option.Some<Transactions, ErrorCustom.Error>(transaction)
            : Option.None<Transactions, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound));
    }

    public async Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentWithContextAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        // 1. Get gateway ID
        var gatewayIdResult = await GetPaymentGatewayIdAsync(req.PaymentGateway, ct);

        return await gatewayIdResult.Match(
            some: async gatewayId =>
            {
                // 2. Get correct payment service
                var paymentService = GetPaymentService(req.PaymentGateway);

                Option<CancelPaymentResponse, ErrorCustom.Error> cancelled;

                if (req.PaymentGateway == PaymentGatewayEnum.PayPal)
                {
                    // PayPal doesn't support true "cancel" after creation
                    // Just mark it as cancelled in our system
                    await UpdateTransactionStatusAsync(req.TransactionId, "cancelled", ct);

                    cancelled = Option.Some<CancelPaymentResponse, ErrorCustom.Error>(new CancelPaymentResponse(
                        "cancelled",
                        PaymentGatewayEnum.PayPal.ToString()
                    ));
                }
                else
                {
                    // 3. Ask gateway to cancel
                    cancelled = await paymentService.CancelPaymentAsync(req, ct);

                    // 4. If gateway cancel succeeded, update DB
                    if (cancelled.HasValue)
                        await UpdateTransactionStatusAsync(req.TransactionId, "cancelled", ct);
                }

                return cancelled;
            },
            none: err => Task.FromResult(Option.None<CancelPaymentResponse, ErrorCustom.Error>(err))
        );
    }

    public async Task<Option<Transactions, Error>> UpdateTransactionGatewayInfoAsync(Guid transactionId, string gatewayReference, CancellationToken ct)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
        if (transaction == null)
        {
            return Option.None<Transactions, ErrorCustom.Error>(
                new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound)
            );
        }

        // Store the gateway reference (e.g., PayPal SessionId, Stripe PaymentIntentId, etc.)
        transaction.TransactionReference = gatewayReference;

        await _transactionRepository.UpdateAsync(transaction, ct);

        return Option.Some<Transactions, ErrorCustom.Error>(transaction);
    }
}