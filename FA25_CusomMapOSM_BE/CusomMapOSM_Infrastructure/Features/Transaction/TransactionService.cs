using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Optional;
using Optional.Unsafe;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Infrastructure.Services;
using Microsoft.Extensions.Logging;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Commons.Constant;

namespace CusomMapOSM_Infrastructure.Features.Transaction;

public class TransactionContext
{
    public Guid? UserId { get; set; }
    public Guid? OrgId { get; set; }
    public int? PlanId { get; set; }
    public bool AutoRenew { get; set; } = true;
    public Guid? MembershipId { get; set; }
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentService _paymentService;
    private readonly IMembershipService _membershipService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPaymentGatewayRepository _paymentGatewayRepository;
    private readonly HangfireEmailService _hangfireEmailService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMembershipPlanRepository _membershipPlanRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IPaymentService paymentService,
        IMembershipService membershipService,
        IServiceProvider serviceProvider,
        IPaymentGatewayRepository paymentGatewayRepository,
        HangfireEmailService hangfireEmailService,
        IEmailNotificationService emailNotificationService,
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        IMembershipPlanRepository membershipPlanRepository, ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _paymentService = paymentService;
        _membershipService = membershipService;
        _serviceProvider = serviceProvider;
        _paymentGatewayRepository = paymentGatewayRepository;
        _hangfireEmailService = hangfireEmailService;
        _emailNotificationService = emailNotificationService;
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
            null, // MembershipId - will be set after membership creation
            null, // ExportId
            "pending",
            ct
        );

        if (!pendingTransactionResult.HasValue)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Transaction.Create.Failed", "Failed to create transaction", ErrorCustom.ErrorType.Failure));

        var pendingTransaction = pendingTransactionResult.ValueOrDefault()!;

        // 3. Store business context in transaction metadata for later retrieval
        await StoreTransactionContextAsync(pendingTransaction.TransactionId, request, ct);

        // 4. Get PaymentService
        _logger.LogInformation("Getting payment service for gateway: {PaymentGateway}", request.PaymentGateway);
        var paymentService = GetPaymentService(request.PaymentGateway);
        _logger.LogInformation("Payment service obtained: {ServiceType}", paymentService.GetType().Name);

        // 5. Create checkout with full request context for multi-item support
        var returnUrl = $"{FrontendConstant.FRONTEND_BASE_URL}/profile/settings/plans?transactionId={pendingTransaction.TransactionId}";
        var cancelUrl = $"{FrontendConstant.FRONTEND_BASE_URL}/profile/settings/plans?transactionId={pendingTransaction.TransactionId}";

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

        var approval = checkoutResult.ValueOrDefault()!;
        _logger.LogInformation("Checkout created successfully - SessionId: {SessionId}, ApprovalUrl: {ApprovalUrl}",
            approval.SessionId, approval.ApprovalUrl);

        // 6. Save gateway session/payment ID
        await UpdateTransactionGatewayInfoAsync(
            pendingTransaction.TransactionId,
            approval.SessionId,
            ct
        );

        // 7. Store plan snapshot and payment info in transaction Content
        if (request.PlanId.HasValue)
        {
            var plan = await _membershipPlanRepository.GetPlanByIdAsync(request.PlanId.Value, ct);
            if (plan != null)
            {
                var planSnapshot = new
                {
                    PlanId = plan.PlanId,
                    PlanName = plan.PlanName,
                    Description = plan.Description,
                    PriceMonthly = plan.PriceMonthly,
                    DurationMonths = plan.DurationMonths,
                    MaxLocationsPerOrg = plan.MaxLocationsPerOrg,
                    MaxMapsPerMonth = plan.MaxMapsPerMonth,
                    MaxUsersPerOrg = plan.MaxUsersPerOrg,
                    MapQuota = plan.MapQuota,
                    ExportQuota = plan.ExportQuota,
                    MaxCustomLayers = plan.MaxCustomLayers,
                    MonthlyTokens = plan.MonthlyTokens,
                    PrioritySupport = plan.PrioritySupport,
                    Features = plan.Features, // JSON string
                    MaxInteractionsPerMap = plan.MaxInteractionsPerMap,
                    MaxMediaFileSizeBytes = plan.MaxMediaFileSizeBytes,
                    MaxVideoFileSizeBytes = plan.MaxVideoFileSizeBytes,
                    MaxAudioFileSizeBytes = plan.MaxAudioFileSizeBytes,
                    MaxConnectionsPerMap = plan.MaxConnectionsPerMap,
                    Allow3DEffects = plan.Allow3DEffects,
                    AllowVideoContent = plan.AllowVideoContent,
                    AllowAudioContent = plan.AllowAudioContent,
                    AllowAnimatedConnections = plan.AllowAnimatedConnections,
                    IsActive = plan.IsActive,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt
                };

                var transactionContent = new
                {
                    Purpose = request.Purpose,
                    Context = new
                    {
                        UserId = request.UserId,
                        OrgId = request.OrgId,
                        PlanId = request.PlanId,
                        AutoRenew = request.AutoRenew
                    },
                    PlanSnapshot = planSnapshot,
                    PaymentInfo = new
                    {
                        Status = "pending",
                        PaymentUrl = approval.ApprovalUrl,
                        ReturnUrl = returnUrl,
                        LastUpdatedAt = DateTime.UtcNow
                    }
                };

                pendingTransaction.Content = System.Text.Json.JsonSerializer.Serialize(transactionContent);
                await _transactionRepository.UpdateAsync(pendingTransaction, ct);
            }
        }

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
            if (service is PayOSPaymentService && gateway == PaymentGatewayEnum.PayOS)
                return service;
        }

        throw new ArgumentException($"Payment gateway '{gateway}' is not supported or not properly registered");
    }

    public async Task<Option<object, ErrorCustom.Error>> ConfirmPaymentWithContextAsync(
        ConfirmPaymentWithContextReq req,
        CancellationToken ct)
    {
        _logger.LogInformation("=== ConfirmPaymentWithContextAsync START ===");
        _logger.LogInformation("Purpose: {Purpose}, TransactionId: {TransactionId}, PaymentGateway: {PaymentGateway}, PaymentId: {PaymentId}, OrderCode: {OrderCode}",
            req.Purpose, req.TransactionId, req.PaymentGateway, req.PaymentId, req.OrderCode);

        // 1. If we already have a TransactionId, reuse it
        Transactions transaction;

        if (req.TransactionId != Guid.Empty)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(req.TransactionId, ct);
            if (existingTransaction is null)
            {
                _logger.LogError("Transaction not found: {TransactionId}", req.TransactionId);
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Transaction.NotFound", "Transaction not found", ErrorCustom.ErrorType.NotFound));
            }

            _logger.LogInformation("Found existing transaction: {TransactionId}, Purpose: {Purpose}", 
                existingTransaction.TransactionId, existingTransaction.Purpose);
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

            transaction = initialTransactionResult.ValueOrDefault()!;
        }

        // 2. Get correct payment service
        var paymentService = GetPaymentService(req.PaymentGateway);

        // 3. Confirm payment
        _logger.LogInformation("Confirming payment with PayOS - PaymentId: {PaymentId}, OrderCode: {OrderCode}", 
            req.PaymentId, req.OrderCode);
        
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
                _logger.LogInformation("Payment confirmed successfully, updating transaction status");
                await UpdateTransactionStatusAsync(transaction.TransactionId, "success", ct);

                // 4. Post-payment fulfillment logic using stored context
                _logger.LogInformation("Processing post-payment fulfillment for purpose: {Purpose}", req.Purpose);
                var fulfillmentResult = await HandlePostPaymentWithStoredContextAsync(transaction, ct);
                _logger.LogInformation("=== ConfirmPaymentWithContextAsync SUCCESS ===");
                return fulfillmentResult;
            },
            none: async err =>
            {
                _logger.LogError("Payment confirmation failed: {ErrorCode} - {ErrorMessage}", 
                    err.Code, err);
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
                    // Update transaction with the created membership ID and content
                    transaction.MembershipId = m.MembershipId;

                    // Save transaction content when transaction completes
                    var plan = await _membershipPlanRepository.GetPlanByIdAsync(context.PlanId.Value, ct);
                    transaction.Content = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Purpose = purpose,
                        PlanId = context.PlanId.Value,
                        PlanName = plan?.PlanName ?? "Unknown Plan",
                        MembershipId = m.MembershipId,
                        Amount = transaction.Amount,
                        BillingCycleStartDate = m.BillingCycleStartDate,
                        BillingCycleEndDate = m.BillingCycleEndDate,
                        ProcessedAt = DateTime.UtcNow
                    });

                    await _transactionRepository.UpdateAsync(transaction, ct);


                    // Send purchase confirmation notification
                    var user = await _userRepository.GetUserByIdAsync(context.UserId.Value, ct);
                    await _emailNotificationService.SendTransactionCompletedNotificationAsync(
                        user?.Email ?? "unknown@example.com",
                        user?.FullName ?? "User",
                        transaction.Amount,
                        m.Plan?.PlanName ?? "Unknown Plan");

                    return Option.Some<object, ErrorCustom.Error>(new
                    {
                        MembershipId = m.MembershipId,
                        TransactionId = transaction.TransactionId
                    });
                },
                none: err => Task.FromResult(Option.None<object, ErrorCustom.Error>(err))
            );
        }

        if (string.Equals(purpose, "upgrade", StringComparison.OrdinalIgnoreCase))
        {
            if (context?.UserId is null || context?.OrgId is null || context?.PlanId is null)
                return Option.None<object, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Context.Invalid", "Missing upgrade context", ErrorCustom.ErrorType.Validation));

            // Check if membership is already on the target plan (idempotency check)
            // This can happen if this method is called multiple times for the same transaction
            var currentMembership = await _membershipService.GetCurrentMembershipWithIncludesAsync(
                context.UserId.Value, context.OrgId.Value, ct);
            
            if (currentMembership.HasValue && currentMembership.ValueOrDefault().PlanId == context.PlanId.Value)
            {
                // Membership is already on the target plan - upgrade was already processed
                _logger.LogInformation("Upgrade already processed - membership is already on plan {PlanId}. Returning success (idempotency).", context.PlanId.Value);
                
                // Update transaction with existing membership ID if not already set
                if (!transaction.MembershipId.HasValue && currentMembership.HasValue)
                {
                    transaction.MembershipId = currentMembership.ValueOrDefault().MembershipId;
                    await _transactionRepository.UpdateAsync(transaction, ct);
                }
                
                return Option.Some<object, ErrorCustom.Error>(new
                {
                    MembershipId = currentMembership.ValueOrDefault().MembershipId,
                    TransactionId = transaction.TransactionId,
                    Message = "Upgrade already processed"
                });
            }

            var membership = await _membershipService.ChangeSubscriptionPlanAsync(
                context.UserId.Value,
                context.OrgId.Value,
                context.PlanId.Value,
                context.AutoRenew,
                ct
            );

            return await membership.Match(
                some: async m =>
                {
                    // Update transaction with the updated membership ID and content
                    transaction.MembershipId = m.MembershipId;

                    // Save transaction content when transaction completes
                    var plan = await _membershipPlanRepository.GetPlanByIdAsync(context.PlanId.Value, ct);
                    transaction.Content = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Purpose = purpose,
                        PlanId = context.PlanId.Value,
                        PlanName = plan?.PlanName ?? "Unknown Plan",
                        MembershipId = m.MembershipId,
                        Amount = transaction.Amount,
                        BillingCycleStartDate = m.BillingCycleStartDate,
                        BillingCycleEndDate = m.BillingCycleEndDate,
                        ProcessedAt = DateTime.UtcNow
                    });

                    await _transactionRepository.UpdateAsync(transaction, ct);

                    // Send upgrade confirmation notification
                    var user = await _userRepository.GetUserByIdAsync(context.UserId.Value, ct);
                    await _emailNotificationService.SendTransactionCompletedNotificationAsync(
                        user?.Email ?? "unknown@example.com",
                        user?.FullName ?? "User",
                        transaction.Amount,
                        m.Plan?.PlanName ?? "Unknown Plan");

                    return Option.Some<object, ErrorCustom.Error>(new
                    {
                        MembershipId = m.MembershipId,
                        TransactionId = transaction.TransactionId
                    });
                },
                none: async err =>
                {
                    // Check if error is "SamePlan" - this means upgrade was already processed
                    if (err.Code == "Membership.SamePlan")
                    {
                        _logger.LogInformation("Upgrade failed with SamePlan error - membership is already on target plan. Treating as success (idempotency).");
                        
                        // Get current membership to return its ID
                        if (currentMembership.HasValue)
                        {
                            var m = currentMembership.ValueOrDefault();
                            if (!transaction.MembershipId.HasValue)
                            {
                                transaction.MembershipId = m.MembershipId;
                                await _transactionRepository.UpdateAsync(transaction, ct);
                            }
                            
                            return Option.Some<object, ErrorCustom.Error>(new
                            {
                                MembershipId = m.MembershipId,
                                TransactionId = transaction.TransactionId,
                                Message = "Upgrade already processed"
                            });
                        }
                    }
                    
                    // Real error - return it
                    return Option.None<object, ErrorCustom.Error>(err);
                }
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
                return (transaction.Purpose ?? string.Empty, null);
            }

            var parts = transaction.Purpose.Split('|', 2);
            Console.WriteLine($"Split parts count: {parts.Length}");

            if (parts.Length != 2)
            {
                Console.WriteLine($"Invalid parts count, returning: ({transaction.Purpose}, null)");
                Console.WriteLine($"=== End Parsing Transaction Context ===");
                return (transaction.Purpose ?? string.Empty, null);
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

    public async Task<Option<object, ErrorCustom.Error>> HandleWebhookAsync(PaymentGatewayEnum gateway, string gatewayReference, string? orderCode = null, CancellationToken ct = default)
    {
        _logger.LogInformation("=== TransactionService.HandleWebhookAsync START ===");
        _logger.LogInformation("Gateway: {Gateway}, GatewayReference: {GatewayReference}, OrderCode: {OrderCode}", gateway, gatewayReference, orderCode);

        // 1. Find transaction by gateway reference (TransactionReference field)
        var transaction = await _transactionRepository.GetByTransactionReferenceAsync(gatewayReference, ct);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for gateway reference: {GatewayReference}", gatewayReference);
            return Option.None<object, ErrorCustom.Error>(
                new ErrorCustom.Error("Transaction.NotFound", "Transaction not found for the given gateway reference", ErrorCustom.ErrorType.NotFound));
        }

        _logger.LogInformation("Transaction found: {TransactionId}, Status: {Status}", transaction.TransactionId, transaction.Status);

        // 2. Check if transaction is already processed
        if (transaction.Status == "success")
        {
            _logger.LogInformation("Transaction already processed successfully: {TransactionId}", transaction.TransactionId);
            return Option.Some<object, ErrorCustom.Error>(new
            {
                TransactionId = transaction.TransactionId,
                Status = "success",
                Message = "Transaction already processed"
            });
        }

        // 3. Get payment service and verify payment status
        var paymentService = GetPaymentService(gateway);
        
        // Extract payment details based on gateway
        // For PayOS, OrderCode is required and should be passed from webhook
        var confirmRequest = new ConfirmPaymentReq
        {
            PaymentGateway = gateway,
            PaymentId = gatewayReference,
            OrderCode = orderCode // PayOS requires this, other gateways can ignore it
        };

        var confirmed = await paymentService.ConfirmPaymentAsync(confirmRequest, ct);

        return await confirmed.Match(
            some: async _ =>
            {
                // 4. Update transaction status
                await UpdateTransactionStatusAsync(transaction.TransactionId, "success", ct);

                // 5. Handle post-payment fulfillment
                var fulfillmentResult = await HandlePostPaymentWithStoredContextAsync(transaction, ct);
                _logger.LogInformation("=== TransactionService.HandleWebhookAsync SUCCESS ===");
                return fulfillmentResult;
            },
            none: async err =>
            {
                // Update transaction status to failed if payment verification failed
                if (transaction.Status == "pending")
                {
                    await UpdateTransactionStatusAsync(transaction.TransactionId, "failed", ct);
                }
                return Option.None<object, ErrorCustom.Error>(err);
            }
        );
    }




}