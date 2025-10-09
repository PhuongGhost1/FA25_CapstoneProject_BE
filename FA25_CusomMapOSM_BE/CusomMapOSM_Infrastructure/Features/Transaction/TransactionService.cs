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
using CusomMapOSM_Infrastructure.Services;
using Microsoft.Extensions.Logging;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases;

namespace CusomMapOSM_Infrastructure.Features.Transaction;

public class TransactionContext
{
    public Guid? UserId { get; set; }
    public Guid? OrgId { get; set; }
    public int? PlanId { get; set; }
    public bool AutoRenew { get; set; } = true;
    public Guid? MembershipId { get; set; }
    public string? AddonKey { get; set; }
    public int? Quantity { get; set; }
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentService _paymentService;
    private readonly IMembershipService _membershipService;
    private readonly IUserAccessToolService _userAccessToolService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPaymentGatewayRepository _paymentGatewayRepository;
    private readonly HangfireEmailService _hangfireEmailService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMembershipPlanRepository _membershipPlanRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IPaymentService paymentService,
        IMembershipService membershipService,
        IUserAccessToolService userAccessToolService,
        IServiceProvider serviceProvider,
        IPaymentGatewayRepository paymentGatewayRepository,
        HangfireEmailService hangfireEmailService,
        INotificationService notificationService,
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        IMembershipPlanRepository membershipPlanRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _paymentService = paymentService;
        _membershipService = membershipService;
        _userAccessToolService = userAccessToolService;
        _serviceProvider = serviceProvider;
        _paymentGatewayRepository = paymentGatewayRepository;
        _hangfireEmailService = hangfireEmailService;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _membershipPlanRepository = membershipPlanRepository;
        _logger = logger;
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> ProcessPaymentAsync(ProcessPaymentReq request, CancellationToken ct)
    {
        _logger.LogInformation("=== TransactionService.ProcessPaymentAsync START ===");
        _logger.LogInformation("Request Details: Total={Total}, PaymentGateway={PaymentGateway}, Purpose={Purpose}, UserId={UserId}, OrgId={OrgId}, PlanId={PlanId}",
            request.Total, request.PaymentGateway, request.Purpose, request.UserId, request.OrgId, request.PlanId);

        // 1. Get Gateway ID
        var gatewayIdResult = GetPaymentGatewayId(request.PaymentGateway);
        if (!gatewayIdResult.HasValue)
        {
            _logger.LogError("Payment gateway not found: {PaymentGateway}", request.PaymentGateway);
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorCustom.ErrorType.NotFound));
        }

        var gatewayId = gatewayIdResult.ValueOr(default(Guid));
        _logger.LogInformation("Gateway ID found: {GatewayId}", gatewayId);

        // 2. Create pending transaction with business context
        var pendingTransactionResult = await CreateTransactionRecordAsync(
            gatewayId,
            request.Total,
            request.Purpose,
            request.MembershipId, // This will be null for new memberships, which is correct
            null, // ExportId
            "pending",
            ct
        );

        if (!pendingTransactionResult.HasValue)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.Create.Failed", "Failed to create transaction", ErrorCustom.ErrorType.Failure));

        var pendingTransaction = pendingTransactionResult.ValueOr(default(Transactions));

        // 3. Store business context in transaction metadata for later retrieval
        await StoreTransactionContextAsync(pendingTransaction.TransactionId, request, ct);

        // 4. Get PaymentService
        _logger.LogInformation("Getting payment service for gateway: {PaymentGateway}", request.PaymentGateway);
        var paymentService = GetPaymentService(request.PaymentGateway);
        _logger.LogInformation("Payment service obtained: {ServiceType}", paymentService.GetType().Name);

        // 5. Create checkout with full request context for multi-item support
        var returnUrl = $"https://localhost:3000/select-plans?transactionId={pendingTransaction.TransactionId}";
        var cancelUrl = $"https://localhost:3000/select-plans?transactionId={pendingTransaction.TransactionId}";

        _logger.LogInformation("Creating checkout with URLs - ReturnUrl: {ReturnUrl}, CancelUrl: {CancelUrl}", returnUrl, cancelUrl);

        var checkoutResult = await paymentService.CreateCheckoutAsync(
            request,
            returnUrl,
            cancelUrl,
            ct
        );

        if (!checkoutResult.HasValue)
        {
            _logger.LogError("Checkout creation failed for transaction: {TransactionId}", pendingTransaction.TransactionId);
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Checkout.Failed", "Failed to create checkout", ErrorCustom.ErrorType.Failure));
        }

        var approval = checkoutResult.ValueOr(default(ApprovalUrlResponse));
        _logger.LogInformation("Checkout created successfully - SessionId: {SessionId}, ApprovalUrl: {ApprovalUrl}",
            approval.SessionId, approval.ApprovalUrl);

        // 6. Save gateway session/payment ID
        await UpdateTransactionGatewayInfoAsync(
            pendingTransaction.TransactionId,
            approval.SessionId,
            ct
        );

        _logger.LogInformation("=== TransactionService.ProcessPaymentAsync SUCCESS ===");
        return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(approval);
    }

    public IPaymentService GetPaymentService(PaymentGatewayEnum gateway)
    {
        // Get all registered payment services from the service provider
        var paymentServices = _serviceProvider.GetServices<IPaymentService>();

        // Find the service that matches the requested gateway
        foreach (var service in paymentServices)
        {
            // Test the service by trying to create a minimal checkout to see which gateway it supports
            // This is a simple way to identify which service handles which gateway
            if (service is PayOSPaymentService && gateway == PaymentGatewayEnum.PayOS)
                return service;
            if (service is VNPayPaymentService && gateway == PaymentGatewayEnum.VNPay)
                return service;
            if (service is StripePaymentService && gateway == PaymentGatewayEnum.Stripe)
                return service;
            if (service is PaypalPaymentService && gateway == PaymentGatewayEnum.PayPal)
                return service;
        }

        throw new ArgumentException($"Payment gateway '{gateway}' is not supported or not properly registered");
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
            var gatewayIdResult = GetPaymentGatewayId(req.PaymentGateway);
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

                // 4. Post-payment fulfillment logic using stored context
                return await HandlePostPaymentWithStoredContextAsync(transaction, ct);
            },
            none: async err =>
            {
                await UpdateTransactionStatusAsync(transaction.TransactionId, "failed", ct);
                return Option.None<object, ErrorCustom.Error>(err);
            }
        );
    }

    private async Task<Option<object, ErrorCustom.Error>> HandlePostPaymentWithStoredContextAsync(Transactions transaction, CancellationToken ct)
    {
        var (purpose, context) = ParseTransactionContext(transaction);

        if (string.Equals(purpose, "membership", StringComparison.OrdinalIgnoreCase))
        {
            if (context?.UserId is null || context?.OrgId is null || context?.PlanId is null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Context.Invalid", "Missing membership context", ErrorCustom.ErrorType.Validation));

            var membership = await _membershipService.CreateOrRenewMembershipAsync(
                context.UserId.Value,
                context.OrgId.Value,
                context.PlanId.Value,
                context.AutoRenew,
                ct
            );

            return await membership.Match(
                some: async m =>
                {
                    // Grant access tools based on the membership plan
                    var accessToolResult = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(
                        context.UserId.Value,
                        context.PlanId.Value,
                        m.EndDate ?? DateTime.UtcNow.AddMonths(1), // Default to 1 month if no end date
                        ct
                    );

                    accessToolResult.Match(
                        some: _ => { /* Success - do nothing */ },
                        none: error => Console.WriteLine($"Failed to grant access tools: {error?.Description ?? "Unknown error"}")
                    );

                    // Send purchase confirmation notification
                    var user = await _userRepository.GetUserByIdAsync(context.UserId.Value, ct);
                    await _notificationService.SendTransactionCompletedNotificationAsync(
                        user?.Email ?? "unknown@example.com",
                        user?.FullName ?? "User",
                        transaction.Amount,
                        m.Plan?.PlanName ?? "Unknown Plan");

                    return Option.Some<object, ErrorCustom.Error>(new
                    {
                        MembershipId = m.MembershipId,
                        TransactionId = transaction.TransactionId,
                        AccessToolsGranted = accessToolResult.HasValue
                    });
                },
                none: err => Task.FromResult(Option.None<object, ErrorCustom.Error>(err))
            );
        }

        if (string.Equals(purpose, "addon", StringComparison.OrdinalIgnoreCase))
        {
            if (context?.MembershipId is null || context?.OrgId is null || string.IsNullOrWhiteSpace(context?.AddonKey))
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Context.Invalid", "Missing addon context", ErrorCustom.ErrorType.Validation));

            var user = await _userRepository.GetUserByIdAsync(context.UserId.Value, ct);
            if (user == null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("User.NotFound", "User not found", ErrorCustom.ErrorType.NotFound));

            var membership = await _membershipRepository.GetByIdAsync(context.MembershipId.Value, ct);
            if (membership == null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Membership.NotFound", "Membership not found", ErrorCustom.ErrorType.NotFound));

            var plan = await _membershipPlanRepository.GetPlanByIdAsync(membership.PlanId, ct);
            if (plan == null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Plan.NotFound", "Plan not found", ErrorCustom.ErrorType.NotFound));


            var addon = await _membershipService.AddAddonAsync(
                context.MembershipId.Value,
                context.OrgId.Value,
                context.AddonKey,
                context.Quantity,
                true,
                ct
            );

            return await addon.Match(
                some: async a =>
                {
                    // Send addon purchase confirmation email immediately
                    await _notificationService.SendTransactionCompletedNotificationAsync(user!.Email, user.FullName ?? "User", transaction.Amount, plan!.PlanName ?? "Unknown Plan");

                    return Option.Some<object, ErrorCustom.Error>(new
                    {
                        AddonId = a.AddonId,
                        TransactionId = transaction.TransactionId
                    });
                },
                none: err => Task.FromResult(Option.None<object, ErrorCustom.Error>(err))
            );
        }

        return Option.Some<object, ErrorCustom.Error>(new
        {
            Status = "ok",
            TransactionId = transaction.TransactionId
        });
    }

    public Option<Guid, ErrorCustom.Error> GetPaymentGatewayId(PaymentGatewayEnum paymentGateway)
    {
        // Use the predefined GUIDs from PaymentGatewayConfiguration instead of database lookup
        var gatewayId = GetPaymentGatewayIdInternal(paymentGateway);
        if (gatewayId == Guid.Empty)
            return Option.None<Guid, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorCustom.ErrorType.NotFound));

        return Option.Some<Guid, ErrorCustom.Error>(gatewayId);
    }

    private static Guid GetPaymentGatewayIdInternal(PaymentGatewayEnum paymentGateway)
    {
        return paymentGateway switch
        {
            PaymentGatewayEnum.VNPay => SeedDataConstants.VnPayPaymentGatewayId,
            PaymentGatewayEnum.PayPal => SeedDataConstants.PayPalPaymentGatewayId,
            PaymentGatewayEnum.Stripe => SeedDataConstants.StripePaymentGatewayId,
            PaymentGatewayEnum.BankTransfer => SeedDataConstants.BankTransferPaymentGatewayId,
            PaymentGatewayEnum.PayOS => SeedDataConstants.PayOSPaymentGatewayId,
            _ => Guid.Empty
        };
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
        var gatewayIdResult = GetPaymentGatewayId(req.PaymentGateway);

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

    private async Task<Option<bool, ErrorCustom.Error>> StoreTransactionContextAsync(Guid transactionId, ProcessPaymentReq request, CancellationToken ct)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
            if (transaction == null)
            {
                return Option.None<bool, ErrorCustom.Error>(
                    new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound)
                );
            }

            var context = new TransactionContext
            {
                UserId = request.UserId,
                OrgId = request.OrgId,
                PlanId = request.PlanId,
                AutoRenew = request.AutoRenew,
                AddonKey = request.AddonKey,
                Quantity = request.Quantity
            };

            // Store the purpose and context as JSON with a separator
            var contextJson = System.Text.Json.JsonSerializer.Serialize(context);
            transaction.Purpose = $"{request.Purpose}|{contextJson}";

            Console.WriteLine($"=== Storing Transaction Context ===");
            Console.WriteLine($"TransactionId: {transactionId}");
            Console.WriteLine($"Purpose: {request.Purpose}");
            Console.WriteLine($"Context JSON: {contextJson}");
            Console.WriteLine($"Final Purpose Field: {transaction.Purpose}");
            Console.WriteLine($"=== End Storing Transaction Context ===");

            await _transactionRepository.UpdateAsync(transaction, ct);
            return Option.Some<bool, ErrorCustom.Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("Transaction.Context.StoreFailed", $"Failed to store transaction context: {ex.Message}", ErrorCustom.ErrorType.Failure)
            );
        }
    }

    private (string Purpose, TransactionContext? Context) ParseTransactionContext(Transactions transaction)
    {
        try
        {
            Console.WriteLine($"=== Parsing Transaction Context ===");
            Console.WriteLine($"TransactionId: {transaction.TransactionId}");
            Console.WriteLine($"Purpose Field: '{transaction.Purpose}'");
            Console.WriteLine($"Contains '|': {transaction.Purpose?.Contains("|")}");

            if (string.IsNullOrEmpty(transaction.Purpose) || !transaction.Purpose.Contains("|"))
            {
                Console.WriteLine($"No separator found or empty purpose, returning: ({transaction.Purpose}, null)");
                Console.WriteLine($"=== End Parsing Transaction Context ===");
                return (transaction.Purpose, null);
            }

            var parts = transaction.Purpose.Split('|', 2);
            Console.WriteLine($"Split parts count: {parts.Length}");

            if (parts.Length != 2)
            {
                Console.WriteLine($"Invalid parts count, returning: ({transaction.Purpose}, null)");
                Console.WriteLine($"=== End Parsing Transaction Context ===");
                return (transaction.Purpose, null);
            }

            var purpose = parts[0];
            var contextJson = parts[1];
            Console.WriteLine($"Extracted Purpose: '{purpose}'");
            Console.WriteLine($"Extracted Context JSON: '{contextJson}'");

            var context = System.Text.Json.JsonSerializer.Deserialize<TransactionContext>(contextJson);
            Console.WriteLine($"Deserialized Context: UserId={context?.UserId}, OrgId={context?.OrgId}, PlanId={context?.PlanId}");
            Console.WriteLine($"=== End Parsing Transaction Context ===");

            return (purpose, context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== Parsing Transaction Context ERROR ===");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"=== End Parsing Transaction Context ERROR ===");
            return (transaction.Purpose, null);
        }
    }




}