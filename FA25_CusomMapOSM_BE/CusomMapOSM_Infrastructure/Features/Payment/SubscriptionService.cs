using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Application.Common.Errors;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using Optional;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Optional.Unsafe;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Application.Services.Billing;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Features.Payment;

public class SubscriptionService : ISubscriptionService
{
    private readonly ITransactionService _transactionService;
    private readonly IMembershipService _membershipService;
    private readonly INotificationService _notificationService;
    private readonly IMembershipPlanRepository _membershipPlanRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPaymentGatewayRepository _paymentGatewayRepository;
    private readonly IProrationService _prorationService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ITransactionService transactionService,
        IMembershipService membershipService,
        INotificationService notificationService,
        IMembershipPlanRepository membershipPlanRepository,
        IOrganizationRepository organizationRepository,
        ITransactionRepository transactionRepository,
        IMembershipRepository membershipRepository,
        IPaymentGatewayRepository paymentGatewayRepository,
        IProrationService prorationService,
        ILogger<SubscriptionService> logger)
    {
        _transactionService = transactionService;
        _membershipService = membershipService;
        _notificationService = notificationService;
        _membershipPlanRepository = membershipPlanRepository;
        _organizationRepository = organizationRepository;
        _transactionRepository = transactionRepository;
        _membershipRepository = membershipRepository;
        _paymentGatewayRepository = paymentGatewayRepository;
        _prorationService = prorationService;
        _logger = logger;
    }

    public async Task<Option<SubscribeResponse, Error>> SubscribeToPlanAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        try
        {
            // Check if user is the owner of the organization
            var userOrgMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(request.UserId, request.OrgId);
            if (userOrgMember is null || userOrgMember.Role.ToString() != "Owner")
            {
                return Option.None<SubscribeResponse, Error>(Error.Forbidden("Organization.NotOwner", "Only the organization owner can purchase memberships for this organization"));
            }

            // Get plan details to calculate amount
            var planResult = await _membershipPlanRepository.GetPlanByIdAsync(request.PlanId, ct);
            if (planResult is null)
            {
                return Option.None<SubscribeResponse, Error>(Error.NotFound("Plan.NotFound", "Plan not found"));
            }

            var plan = planResult;
            var amount = plan.PriceMonthly ?? 0;

            // Create payment transaction using existing TransactionService
            var paymentRequest = new ProcessPaymentReq
            {
                UserId = request.UserId,
                OrgId = request.OrgId,
                Total = amount,
                PaymentGateway = request.PaymentMethod,
                Purpose = "membership",
                PlanId = request.PlanId,
                AutoRenew = request.AutoRenew,
            };

            var transactionResult = await _transactionService.ProcessPaymentAsync(paymentRequest, ct);
            if (!transactionResult.HasValue)
            {
                return Option.None<SubscribeResponse, Error>(Error.Failure("Payment.ProcessFailed", "Failed to process payment"));
            }

            var approval = transactionResult.ValueOrDefault();

            // Create pending payment notification
            if (plan != null)
            {
                // Get transaction by SessionId (stored as TransactionReference)
                var transaction = await _transactionRepository.GetByTransactionReferenceAsync(approval.SessionId, ct);
                if (transaction != null)
                {
                    var planSummary = $"Maps: {plan.MapQuota}, Exports: {plan.ExportQuota}, Members: {plan.MaxUsersPerOrg}";
                    
                    await _notificationService.CreateTransactionPendingNotificationAsync(
                        request.UserId,
                        transaction.TransactionId,
                        plan.PlanName,
                        planSummary,
                        approval.ApprovalUrl,
                        ct);
                }
            }

            return Option.Some<SubscribeResponse, Error>(new SubscribeResponse
            {
                PaymentGateway = approval.PaymentGateway,
                QrCode = approval.QrCode,
                OrderCode = approval.OrderCode,
                TransactionId = approval.SessionId, // Use SessionId as transaction identifier
                PaymentUrl = approval.ApprovalUrl,
                Status = "pending", // Payment is pending until confirmed
                Message = "Payment initiated successfully. Complete payment to activate your subscription."
            });
        }
        catch (Exception ex)
        {
            return Option.None<SubscribeResponse, Error>(Error.Failure("Payment.SubscribeFailed", $"Failed to subscribe to plan: {ex.Message}"));
        }
    }

    public async Task<Option<UpgradeResponse, Error>> UpgradePlanAsync(UpgradeRequest request, CancellationToken ct = default)
    {
        try
        {
            // Get current membership
            var currentMembershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(request.UserId, request.OrgId, ct);
            if (!currentMembershipResult.HasValue)
            {
                return Option.None<UpgradeResponse, Error>(Error.NotFound("Membership.NotFound", "No active membership found"));
            }

            // Get new plan details
            var newPlanResult = await _membershipPlanRepository.GetPlanByIdAsync(request.NewPlanId, ct);
            if (newPlanResult is null)
            {
                return Option.None<UpgradeResponse, Error>(Error.NotFound("Plan.NotFound", "New plan not found"));
            }

            var currentMembership = currentMembershipResult.ValueOrDefault();
            var newPlan = newPlanResult;
            var currentPlan = currentMembership.Plan;
            
            if (currentPlan == null)
            {
                return Option.None<UpgradeResponse, Error>(Error.NotFound("Plan.CurrentPlanNotFound", "Current plan not found"));
            }

            // Validate upgrade (prevent downgrade - downgrades are NOT allowed)
            // Policy: Users cannot downgrade during billing cycle. If they don't renew, plan downgrades to free plan.
            var currentPlanPrice = currentPlan.PriceMonthly ?? 0;
            var newPlanPrice = newPlan.PriceMonthly ?? 0;
            
            if (newPlanPrice <= currentPlanPrice)
            {
                return Option.None<UpgradeResponse, Error>(
                    Error.ValidationError("Plan.DowngradeNotAllowed", 
                        $"Downgrades are not allowed during billing cycle. Your current plan expires on {currentMembership.BillingCycleEndDate:yyyy-MM-dd}. If you don't renew, your plan will automatically downgrade to the free plan."));
            }

            // Calculate proration using new service
            var proration = _prorationService.CalculateUpgradeProration(
                currentPlanPrice: currentPlanPrice,
                newPlanPrice: newPlanPrice,
                billingCycleStartDate: currentMembership.BillingCycleStartDate,
                billingCycleEndDate: currentMembership.BillingCycleEndDate,
                upgradeDate: DateTime.UtcNow
            );

            // Create payment transaction
            var paymentRequest = new ProcessPaymentReq
            {
                UserId = request.UserId,
                OrgId = request.OrgId,
                Total = proration.AmountDue,
                PaymentGateway = request.PaymentMethod,
                Purpose = "upgrade",
                PlanId = request.NewPlanId,
                AutoRenew = request.AutoRenew
            };

            var transactionResult = await _transactionService.ProcessPaymentAsync(paymentRequest, ct);
            if (!transactionResult.HasValue)
            {
                return Option.None<UpgradeResponse, Error>(Error.Failure("Payment.ProcessFailed", "Failed to process payment"));
            }

            var approval = transactionResult.ValueOrDefault();

            return Option.Some<UpgradeResponse, Error>(new UpgradeResponse
            {
                PaymentGateway = approval.PaymentGateway,
                QrCode = approval.QrCode,
                OrderCode = approval.OrderCode,
                TransactionId = approval.SessionId,
                PaymentUrl = approval.ApprovalUrl,
                Status = "pending",
                Message = proration.Message ?? "Payment initiated successfully. Complete payment to upgrade your plan.",
                ProRatedAmount = proration.AmountDue,
                UnusedCredit = proration.UnusedCredit,
                ProratedNewPlanCost = proration.ProratedNewPlanCost,
                DaysRemaining = proration.DaysRemaining
            });
        }
        catch (Exception ex)
        {
            return Option.None<UpgradeResponse, Error>(Error.Failure("Payment.UpgradeFailed", $"Failed to upgrade plan: {ex.Message}"));
        }
    }


    public async Task<Option<PaymentConfirmationResponse, Error>> ProcessSuccessfulPaymentAsync(Guid transactionId, CancellationToken ct = default)
    {
        try
        {
            // Get transaction details
            var transactionResult = await _transactionService.GetTransactionAsync(transactionId, ct);
            if (!transactionResult.HasValue)
            {
                return Option.None<PaymentConfirmationResponse, Error>(Error.NotFound("Transaction.NotFound", "Transaction not found"));
            }

            var transaction = transactionResult.ValueOrDefault();
            var (purpose, context) = ParseTransactionContext(transaction);

            var response = new PaymentConfirmationResponse
            {
                TransactionId = transactionId.ToString(),
                Status = "success",
                Message = "Payment processed successfully",
                MembershipUpdated = false,
                NotificationSent = false
            };

            // Process based on purpose type
            if (context != null)
            {
                switch (purpose.ToLower())
                {
                    case "membership":
                        if (context.UserId.HasValue && context.OrgId.HasValue && context.PlanId.HasValue)
                        {
                            var createResult = await _membershipService.CreateOrRenewMembershipAsync(
                                context.UserId.Value, context.OrgId.Value, context.PlanId.Value, context.AutoRenew, ct);
                            response.MembershipUpdated = createResult.HasValue;
                        }
                        break;

                    case "upgrade":
                        if (context.UserId.HasValue && context.OrgId.HasValue && context.PlanId.HasValue)
                        {
                            // Check if membership is already on the target plan (idempotency check)
                            // This can happen if HandlePostPaymentWithStoredContextAsync already processed the upgrade
                            var currentMembershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(
                                context.UserId.Value, context.OrgId.Value, ct);
                            
                            if (currentMembershipResult.HasValue && 
                                currentMembershipResult.ValueOrDefault().PlanId == context.PlanId.Value)
                            {
                                // Membership is already on the target plan - upgrade was already processed
                                Console.WriteLine($"ProcessSuccessfulPaymentAsync - Membership already on plan {context.PlanId.Value}. Treating as success (idempotency).");
                                response.MembershipUpdated = true;
                                response.Message = "Payment processed successfully. Membership was already upgraded.";
                            }
                            else
                            {
                                // Try to upgrade
                                Console.WriteLine($"ProcessSuccessfulPaymentAsync - Attempting to upgrade to plan {context.PlanId.Value}.");
                                var upgradeResult = await _membershipService.ChangeSubscriptionPlanAsync(
                                    context.UserId.Value, context.OrgId.Value, context.PlanId.Value, context.AutoRenew, ct);
                                
                                if (upgradeResult.HasValue)
                                {
                                    Console.WriteLine($"ProcessSuccessfulPaymentAsync - Upgrade successful.");
                                    response.MembershipUpdated = true;
                                }
                                else
                                {
                                    // Check if the error is "SamePlan" - this means upgrade was already processed
                                    // Extract error code to check
                                    var errorCode = upgradeResult.Match(
                                        some: _ => string.Empty,
                                        none: err => err.Code
                                    );
                                    
                                    Console.WriteLine($"ProcessSuccessfulPaymentAsync - Upgrade failed with error code: {errorCode}");
                                    
                                    if (errorCode == "Membership.SamePlan")
                                    {
                                        // Membership is already on target plan - upgrade was already processed
                                        // This is expected when HandlePostPaymentWithStoredContextAsync already upgraded
                                        Console.WriteLine("ProcessSuccessfulPaymentAsync - SamePlan error detected. Treating as success (idempotency).");
                                        response.MembershipUpdated = true;
                                        response.Message = "Payment processed successfully. Membership was already upgraded.";
                                    }
                                    else
                                    {
                                        // Real error - membership update failed
                                        Console.WriteLine($"ProcessSuccessfulPaymentAsync - Real error: {errorCode}. Membership update failed.");
                                        response.MembershipUpdated = false;
                                    }
                                }
                            }
                        }
                        break;

                }
            }

            // Grant access tools if membership was updated
            if (response.MembershipUpdated)
            {
            }

            // Auto-cancel conflicting pending transactions
            int cancelledCount = 0;
            if (response.MembershipUpdated && context?.OrgId.HasValue == true && context?.PlanId.HasValue == true)
            {
                cancelledCount = await AutoCancelConflictingTransactionsAsync(
                    context.OrgId.Value,
                    transactionId,
                    context.PlanId.Value,
                    ct
                );
            }
            response.AutoCancelledTransactions = cancelledCount;

            // Send notification
            if (context?.UserId.HasValue == true)
            {
                var notificationResult = await _notificationService.CreateTransactionCompletedNotificationAsync(
                    context.UserId.Value, transaction.Amount, "Plan Subscription");
                response.NotificationSent = notificationResult.HasValue;
            }

            return Option.Some<PaymentConfirmationResponse, Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<PaymentConfirmationResponse, Error>(Error.Failure("Payment.ProcessFailed", $"Failed to process successful payment: {ex.Message}"));
        }
    }

    private (string Purpose, TransactionContext? Context) ParseTransactionContext(CusomMapOSM_Domain.Entities.Transactions.Transactions transaction)
    {
        try
        {
            if (string.IsNullOrEmpty(transaction.Purpose))
                return (transaction.Purpose, null);

            // Check if Purpose contains the context separator
            if (!transaction.Purpose.Contains("|"))
                return (transaction.Purpose, null);

            var parts = transaction.Purpose.Split('|', 2);
            if (parts.Length != 2)
                return (transaction.Purpose, null);

            var purpose = parts[0];
            var contextJson = parts[1];
            var context = System.Text.Json.JsonSerializer.Deserialize<TransactionContext>(contextJson);

            return (purpose, context);
        }
        catch
        {
            return (transaction.Purpose, null);
        }
    }

    /// <summary>
    /// Maps PaymentGatewayId to PaymentGatewayEnum using predefined constants.
    /// Uses predefined GUID constants to avoid relying on database Name field.
    /// </summary>
    private PaymentGatewayEnum? GetPaymentGatewayEnumFromId(Guid gatewayId, ILogger<SubscriptionService>? logger = null)
    {
        logger?.LogInformation("=== GetPaymentGatewayEnumFromId START ===");
        logger?.LogInformation("Looking up gateway ID: {GatewayId}", gatewayId);
        
        // Log all predefined constants for comparison
        logger?.LogInformation("Predefined Gateway Constants:");
        logger?.LogInformation("  - VNPay: {VnPayId}", SeedDataConstants.VnPayPaymentGatewayId);
        logger?.LogInformation("  - PayPal: {PayPalId}", SeedDataConstants.PayPalPaymentGatewayId);
        logger?.LogInformation("  - Stripe: {StripeId}", SeedDataConstants.StripePaymentGatewayId);
        logger?.LogInformation("  - BankTransfer: {BankTransferId}", SeedDataConstants.BankTransferPaymentGatewayId);
        logger?.LogInformation("  - PayOS: {PayOSId}", SeedDataConstants.PayOSPaymentGatewayId);
        
        PaymentGatewayEnum? result = gatewayId switch
        {
            var id when id == SeedDataConstants.VnPayPaymentGatewayId => PaymentGatewayEnum.VNPay,
            var id when id == SeedDataConstants.PayPalPaymentGatewayId => PaymentGatewayEnum.PayPal,
            var id when id == SeedDataConstants.StripePaymentGatewayId => PaymentGatewayEnum.Stripe,
            var id when id == SeedDataConstants.BankTransferPaymentGatewayId => PaymentGatewayEnum.BankTransfer,
            var id when id == SeedDataConstants.PayOSPaymentGatewayId => PaymentGatewayEnum.PayOS,
            _ => (PaymentGatewayEnum?)null
        };
        
        if (result.HasValue)
        {
            logger?.LogInformation("Gateway enum found: {GatewayEnum}", result.Value);
        }
        else
        {
            logger?.LogWarning("Gateway ID {GatewayId} does not match any predefined constants", gatewayId);
        }
        
        logger?.LogInformation("=== GetPaymentGatewayEnumFromId END ===");
        return result;
    }

    private string GetHumanReadablePurpose(string purpose, TransactionContext? context, DomainMembership.Plan? plan)
    {
        if (string.IsNullOrEmpty(purpose))
            return "Unknown transaction";

        // Extract base purpose (before the pipe separator)
        var basePurpose = purpose.Contains("|") ? purpose.Split('|')[0] : purpose;

        // Create human-readable description based on purpose type
        switch (basePurpose.ToLowerInvariant())
        {
            case "membership":
            case "subscription":
                if (plan != null)
                {
                    return $"Đăng ký gói {plan.PlanName}";
                }
                if (context?.PlanId.HasValue == true)
                {
                    // Try to get plan name from repository if not already loaded
                    // For now, return generic message
                    return "Đăng ký gói thành viên";
                }
                return "Đăng ký gói thành viên";

            case "upgrade":
                if (plan != null)
                {
                    return $"Nâng cấp lên gói {plan.PlanName}";
                }
                return "Nâng cấp gói";

            case "renewal":
            case "renew":
                if (plan != null)
                {
                    return $"Gia hạn gói {plan.PlanName}";
                }
                return "Gia hạn gói";

            case "export":
                return "Xuất dữ liệu";

            default:
                return basePurpose;
        }
    }

    public async Task<Option<List<object>, Error>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId, ct);
            if (transactions.Count == 0)
            {
                return Option.Some<List<object>, Error>(new List<object>()); // Return empty list instead of error
            }

            // Apply pagination
            var paginatedTransactions = transactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paymentHistory = new List<object>();

            foreach (var transaction in paginatedTransactions)
            {
                // Fetch PaymentGateway by ID
                var paymentGateway = await _paymentGatewayRepository.GetByGatewayIdAsync(
                    transaction.PaymentGatewayId, ct);

                // Fetch Membership if exists
                DomainMembership.Membership? membership = null;
                DomainMembership.Plan? plan = null;
                CusomMapOSM_Domain.Entities.Organizations.Organization? organization = null;

                // Parse transaction context to get UserId and OrgId
                var (purpose, context) = ParseTransactionContext(transaction);

                if (context?.UserId.HasValue == true && context?.OrgId.HasValue == true)
                {
                    // Find membership by UserId and OrgId instead of MembershipId
                    membership = await _membershipRepository.GetByUserOrgAsync(context.UserId.Value, context.OrgId.Value, ct);

                    if (membership != null)
                    {
                        // Fetch Plan
                        plan = await _membershipPlanRepository.GetPlanByIdAsync(membership.PlanId, ct);

                        // Fetch Organization
                        organization = await _organizationRepository.GetOrganizationById(membership.OrgId);
                    }
                }
                else if (transaction.MembershipId.HasValue)
                {
                    // Fallback: try to find by MembershipId if context parsing fails
                    membership = await _membershipRepository.GetByIdAsync(transaction.MembershipId.Value, ct);

                    if (membership != null)
                    {
                        // Fetch Plan
                        plan = await _membershipPlanRepository.GetPlanByIdAsync(membership.PlanId, ct);

                        // Fetch Organization
                        organization = await _organizationRepository.GetOrganizationById(membership.OrgId);
                    }
                }

                // Parse transaction content for plan snapshot
                // ✅ FIX: Fetch the planned plan from context.PlanId (target plan) instead of current membership plan
                DomainMembership.Plan? plannedPlan = null;
                if (context?.PlanId.HasValue == true)
                {
                    // Fetch the target plan that the transaction was created for
                    plannedPlan = await _membershipPlanRepository.GetPlanByIdAsync(context.PlanId.Value, ct);
                }

                object? pendingPayment = null;

                if (!string.IsNullOrEmpty(transaction.Content))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(transaction.Content);
                        var root = doc.RootElement;
                        
                        // Try to extract plan snapshot from content
                        if (root.TryGetProperty("PlanSnapshot", out var planSnapshotElement))
                        {
                            plannedPlan = new DomainMembership.Plan
                            {
                                PlanId = planSnapshotElement.TryGetProperty("PlanId", out var planIdProp) ? planIdProp.GetInt32() : 0,
                                PlanName = planSnapshotElement.TryGetProperty("PlanName", out var planNameProp) ? planNameProp.GetString() ?? "Unknown" : "Unknown",
                                Description = planSnapshotElement.TryGetProperty("Description", out var descProp) ? descProp.GetString() : null,
                                PriceMonthly = planSnapshotElement.TryGetProperty("PriceMonthly", out var priceProp) ? priceProp.GetDecimal() : null,
                                DurationMonths = planSnapshotElement.TryGetProperty("DurationMonths", out var durationProp) ? durationProp.GetInt32() : 1,
                                MaxLocationsPerOrg = planSnapshotElement.TryGetProperty("MaxLocationsPerOrg", out var maxLocProp) ? maxLocProp.GetInt32() : 0,
                                MaxMapsPerMonth = planSnapshotElement.TryGetProperty("MaxMapsPerMonth", out var maxMapsProp) ? maxMapsProp.GetInt32() : 0,
                                MaxUsersPerOrg = planSnapshotElement.TryGetProperty("MaxUsersPerOrg", out var maxUsersProp) ? maxUsersProp.GetInt32() : 0,
                                MapQuota = planSnapshotElement.TryGetProperty("MapQuota", out var mapQuotaProp) ? mapQuotaProp.GetInt32() : 0,
                                ExportQuota = planSnapshotElement.TryGetProperty("ExportQuota", out var exportQuotaProp) ? exportQuotaProp.GetInt32() : 0,
                                MaxCustomLayers = planSnapshotElement.TryGetProperty("MaxCustomLayers", out var maxLayersProp) ? maxLayersProp.GetInt32() : 0,
                                MonthlyTokens = planSnapshotElement.TryGetProperty("MonthlyTokens", out var tokensProp) ? tokensProp.GetInt32() : 0,
                                PrioritySupport = planSnapshotElement.TryGetProperty("PrioritySupport", out var priorityProp) ? priorityProp.GetBoolean() : false,
                                Features = planSnapshotElement.TryGetProperty("Features", out var featuresProp) ? featuresProp.GetString() : null,
                                MaxInteractionsPerMap = planSnapshotElement.TryGetProperty("MaxInteractionsPerMap", out var interactionsProp) ? interactionsProp.GetInt32() : 50,
                                MaxMediaFileSizeBytes = planSnapshotElement.TryGetProperty("MaxMediaFileSizeBytes", out var mediaSizeProp) ? mediaSizeProp.GetInt64() : 10_485_760,
                                MaxVideoFileSizeBytes = planSnapshotElement.TryGetProperty("MaxVideoFileSizeBytes", out var videoSizeProp) ? videoSizeProp.GetInt64() : 104_857_600,
                                MaxAudioFileSizeBytes = planSnapshotElement.TryGetProperty("MaxAudioFileSizeBytes", out var audioSizeProp) ? audioSizeProp.GetInt64() : 20_971_520,
                                MaxConnectionsPerMap = planSnapshotElement.TryGetProperty("MaxConnectionsPerMap", out var connectionsProp) ? connectionsProp.GetInt32() : 100,
                                Allow3DEffects = planSnapshotElement.TryGetProperty("Allow3DEffects", out var allow3DProp) ? allow3DProp.GetBoolean() : false,
                                AllowVideoContent = planSnapshotElement.TryGetProperty("AllowVideoContent", out var allowVideoProp) ? allowVideoProp.GetBoolean() : true,
                                AllowAudioContent = planSnapshotElement.TryGetProperty("AllowAudioContent", out var allowAudioProp) ? allowAudioProp.GetBoolean() : true,
                                AllowAnimatedConnections = planSnapshotElement.TryGetProperty("AllowAnimatedConnections", out var allowAnimatedProp) ? allowAnimatedProp.GetBoolean() : true,
                                IsActive = planSnapshotElement.TryGetProperty("IsActive", out var isActiveProp) ? isActiveProp.GetBoolean() : true,
                                CreatedAt = planSnapshotElement.TryGetProperty("CreatedAt", out var createdAtProp) && createdAtProp.TryGetDateTime(out var createdAt) ? createdAt : DateTime.UtcNow,
                                UpdatedAt = planSnapshotElement.TryGetProperty("UpdatedAt", out var updatedAtProp) && updatedAtProp.TryGetDateTime(out var updatedAt) ? updatedAt : null
                            };
                        }
                        
                        // Extract payment info from content for pending transactions
                        if (transaction.Status == "pending" && root.TryGetProperty("PaymentInfo", out var paymentInfoElement))
                        {
                            pendingPayment = new
                            {
                                PaymentUrl = paymentInfoElement.TryGetProperty("PaymentUrl", out var urlProp) ? urlProp.GetString() : null,
                                LastUpdatedAt = paymentInfoElement.TryGetProperty("LastUpdatedAt", out var updatedProp) ? updatedProp.GetString() : null
                            };
                        }
                    }
                    catch
                    {
                        // Ignore parse errors
                    }
                }

                var paymentHistoryItem = new
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    Purpose = transaction.Purpose, // Original purpose field (for debugging)
                    Description = GetHumanReadablePurpose(purpose, context, plannedPlan ?? plan), // Human-readable description
                    TransactionDate = transaction.TransactionDate,
                    CreatedAt = transaction.CreatedAt,
                    TransactionReference = transaction.TransactionReference,
                    CanRetry = transaction.Status == "pending",
                    PaymentGateway = paymentGateway != null ? new
                    {
                        GatewayId = paymentGateway.GatewayId,
                        Name = paymentGateway.Name
                    } : null,
                    PlannedPlan = plannedPlan != null ? new
                    {
                        PlanId = plannedPlan.PlanId,
                        PlanName = plannedPlan.PlanName,
                        Description = plannedPlan.Description,
                        PriceMonthly = plannedPlan.PriceMonthly,
                        DurationMonths = plannedPlan.DurationMonths,
                        MaxLocationsPerOrg = plannedPlan.MaxLocationsPerOrg,
                        MaxMapsPerMonth = plannedPlan.MaxMapsPerMonth,
                        MapQuota = plannedPlan.MapQuota,
                        ExportQuota = plannedPlan.ExportQuota,
                        MaxUsersPerOrg = plannedPlan.MaxUsersPerOrg,
                        MaxCustomLayers = plannedPlan.MaxCustomLayers,
                        MonthlyTokens = plannedPlan.MonthlyTokens,
                        PrioritySupport = plannedPlan.PrioritySupport,
                        Features = plannedPlan.Features,
                        MaxInteractionsPerMap = plannedPlan.MaxInteractionsPerMap,
                        MaxMediaFileSizeBytes = plannedPlan.MaxMediaFileSizeBytes,
                        MaxVideoFileSizeBytes = plannedPlan.MaxVideoFileSizeBytes,
                        MaxAudioFileSizeBytes = plannedPlan.MaxAudioFileSizeBytes,
                        MaxConnectionsPerMap = plannedPlan.MaxConnectionsPerMap,
                        Allow3DEffects = plannedPlan.Allow3DEffects,
                        AllowVideoContent = plannedPlan.AllowVideoContent,
                        AllowAudioContent = plannedPlan.AllowAudioContent,
                        AllowAnimatedConnections = plannedPlan.AllowAnimatedConnections,
                        IsActive = plannedPlan.IsActive,
                        CreatedAt = plannedPlan.CreatedAt,
                        UpdatedAt = plannedPlan.UpdatedAt
                    } : null,
                    PendingPayment = pendingPayment,
                    Membership = membership != null ? new
                    {
                        MembershipId = membership.MembershipId,
                        BillingCycleStartDate = membership.BillingCycleStartDate,
                        BillingCycleEndDate = membership.BillingCycleEndDate,
                        Status = membership.Status.ToString(),
                        AutoRenew = membership.AutoRenew,
                        Plan = plan != null ? new
                        {
                            PlanId = plan.PlanId,
                            PlanName = plan.PlanName,
                            Description = plan.Description,
                            PriceMonthly = plan.PriceMonthly,
                            DurationMonths = plan.DurationMonths
                        } : null,
                        Organization = organization != null ? new
                        {
                            OrgId = organization.OrgId,
                            OrgName = organization.OrgName,
                            Abbreviation = organization.Abbreviation
                        } : null
                    } : null
                };

                paymentHistory.Add(paymentHistoryItem);
            }

            return Option.Some<List<object>, Error>(paymentHistory);
        }
        catch (Exception ex)
        {
            return Option.None<List<object>, Error>(Error.Failure("Payment.HistoryFailed", $"Failed to get payment history: {ex.Message}"));
        }
    }

    public async Task<Option<RetryPaymentResponse, Error>> RetryPaymentAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Get transaction
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
            if (transaction == null)
            {
                return Option.None<RetryPaymentResponse, Error>(
                    Error.NotFound("Transaction.NotFound", "Transaction not found"));
            }

            // 2. Check status - only allow retry for pending transactions
            if (transaction.Status != "pending")
            {
                return Option.None<RetryPaymentResponse, Error>(
                    Error.ValidationError("Transaction.NotPending",
                        $"Only pending transactions can be retried. Current status: {transaction.Status}"));
            }

            // 2.5. Check if existing payment URL is still valid (URL Persistence)
            bool IsPaymentUrlValid()
            {
                if (string.IsNullOrEmpty(transaction.PaymentUrl))
                    return false;
                if (transaction.PaymentUrlExpiresAt == null)
                    return false;
                return transaction.PaymentUrlExpiresAt > DateTime.UtcNow;
            }

            if (IsPaymentUrlValid())
            {
                _logger.LogInformation("Reusing existing payment URL for transaction {TransactionId}", transactionId);
                return Option.Some<RetryPaymentResponse, Error>(new RetryPaymentResponse
                {
                    TransactionId = transactionId,
                    PaymentUrl = transaction.PaymentUrl!,
                    Status = "pending",
                    Message = "Redirecting to existing payment session",
                    ExpiresAt = transaction.PaymentUrlExpiresAt,
                    IsNewUrl = false
                });
            }

            _logger.LogInformation("Payment URL expired or missing for transaction {TransactionId}, creating new payment URL", transactionId);

            // 3. Parse transaction Content to get context
            TransactionContext? context = null;
            string purpose = transaction.Purpose;
            
            if (!string.IsNullOrEmpty(transaction.Content))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(transaction.Content);
                    var root = doc.RootElement;
                    
                    // Extract purpose from Content if available
                    if (root.TryGetProperty("Purpose", out var purposeElement))
                    {
                        purpose = purposeElement.GetString() ?? transaction.Purpose;
                    }
                    
                    // Extract context from Content
                    if (root.TryGetProperty("Context", out var contextElement))
                    {
                        context = System.Text.Json.JsonSerializer.Deserialize<TransactionContext>(contextElement.GetRawText());
                    }
                }
                catch
                {
                    // Fallback to parsing from Purpose field if Content parsing fails
                    var (parsedPurpose, parsedContext) = ParseTransactionContext(transaction);
                    purpose = parsedPurpose;
                    context = parsedContext;
                }
            }
            else
            {
                // Fallback to parsing from Purpose field
                var (parsedPurpose, parsedContext) = ParseTransactionContext(transaction);
                purpose = parsedPurpose;
                context = parsedContext;
            }
            
            if (context?.UserId == null || context.UserId != userId)
            {
                return Option.None<RetryPaymentResponse, Error>(
                    Error.Forbidden("Transaction.Unauthorized",
                        "You do not have permission to retry this transaction"));
            }

            if (context.OrgId == null || context.PlanId == null)
            {
                return Option.None<RetryPaymentResponse, Error>(
                    Error.ValidationError("Transaction.InvalidContext",
                        "Transaction context is missing required information"));
            }

            // 4. Check if PaymentGatewayId is valid (Guid.Empty check)
            _logger.LogInformation("=== RetryPaymentAsync: Gateway Lookup ===");
            _logger.LogInformation("Transaction ID: {TransactionId}", transaction.TransactionId);
            _logger.LogInformation("Transaction PaymentGatewayId: {PaymentGatewayId}", transaction.PaymentGatewayId);
            _logger.LogInformation("Transaction Status: {Status}", transaction.Status);

            if (transaction.PaymentGatewayId == Guid.Empty)
            {
                _logger.LogError("Transaction {TransactionId} has empty PaymentGatewayId", transaction.TransactionId);
                return Option.None<RetryPaymentResponse, Error>(
                    Error.ValidationError("Transaction.InvalidGatewayId",
                        "Transaction does not have a valid payment gateway ID"));
            }

            // 5. Try to map PaymentGatewayId directly to PaymentGatewayEnum using predefined constants
            _logger.LogInformation("Attempting to map PaymentGatewayId {PaymentGatewayId} to PaymentGatewayEnum using predefined constants", transaction.PaymentGatewayId);
            var gatewayEnumNullable = GetPaymentGatewayEnumFromId(transaction.PaymentGatewayId, _logger);

            // 6. If constant lookup fails, try database lookup as fallback
            if (gatewayEnumNullable == null)
            {
                _logger.LogWarning("Gateway ID {GatewayId} not found in predefined constants. Attempting database lookup...", transaction.PaymentGatewayId);
                
                var paymentGateway = await _paymentGatewayRepository.GetByGatewayIdAsync(transaction.PaymentGatewayId, ct);
                if (paymentGateway != null)
                {
                    _logger.LogInformation("Gateway found in database: Name={GatewayName}, Id={GatewayId}", paymentGateway.Name, paymentGateway.GatewayId);
                    
                    // Map gateway name to enum (case-insensitive, trimmed)
                    var gatewayName = (paymentGateway.Name ?? string.Empty).Trim().ToLowerInvariant();
                    _logger.LogInformation("Normalized gateway name: {NormalizedName}", gatewayName);
                    
                    gatewayEnumNullable = gatewayName switch
                    {
                        "payos" => PaymentGatewayEnum.PayOS,
                        "stripe" => PaymentGatewayEnum.Stripe,
                        "vnpay" => PaymentGatewayEnum.VNPay,
                        "paypal" => PaymentGatewayEnum.PayPal,
                        "banktransfer" => PaymentGatewayEnum.BankTransfer,
                        _ => null
                    };
                    
                    if (gatewayEnumNullable.HasValue)
                    {
                        _logger.LogInformation("Successfully mapped gateway name '{GatewayName}' to enum: {GatewayEnum}", paymentGateway.Name, gatewayEnumNullable.Value);
                    }
                    else
                    {
                        _logger.LogError("Gateway name '{GatewayName}' (normalized: '{NormalizedName}') is not supported. Supported names: payos, stripe, vnpay, paypal, banktransfer", 
                            paymentGateway.Name, gatewayName);
                    }
                }
                else
                {
                    _logger.LogError("Gateway ID {GatewayId} not found in database either", transaction.PaymentGatewayId);
                }
            }

            // 7. Final fallback: If gateway cannot be determined, default to PayOS
            // This handles cases where transactions were created with random gateway IDs and empty names
            if (gatewayEnumNullable == null)
            {
                _logger.LogWarning("Failed to map PaymentGatewayId {PaymentGatewayId} to PaymentGatewayEnum using constants or database lookup. Defaulting to PayOS as fallback.", transaction.PaymentGatewayId);
                gatewayEnumNullable = PaymentGatewayEnum.PayOS;
                _logger.LogInformation("Using PayOS as default gateway for transaction {TransactionId}", transaction.TransactionId);
            }

            var gatewayEnum = gatewayEnumNullable.Value;
            _logger.LogInformation("Successfully resolved gateway: {GatewayEnum} for PaymentGatewayId {PaymentGatewayId}", gatewayEnum, transaction.PaymentGatewayId);
            _logger.LogInformation("=== RetryPaymentAsync: Gateway Lookup Complete ===");

            // 8. Rebuild payment request
            var paymentRequest = new ProcessPaymentReq
            {
                UserId = context.UserId.Value,
                OrgId = context.OrgId.Value,
                Total = transaction.Amount,
                PaymentGateway = gatewayEnum,
                Purpose = purpose,
                PlanId = context.PlanId.Value,
                AutoRenew = context.AutoRenew
            };

            // 9. Create new checkout via TransactionService
            var transactionResult = await _transactionService.ProcessPaymentAsync(paymentRequest, ct);
            if (!transactionResult.HasValue)
            {
                return Option.None<RetryPaymentResponse, Error>(
                    Error.Failure("Payment.RetryFailed", "Failed to create retry payment"));
            }

            var approval = transactionResult.ValueOrDefault();

            // 10. Update transaction entity fields with payment URL persistence data
            transaction.PaymentUrl = approval.ApprovalUrl;
            transaction.PaymentUrlCreatedAt = DateTime.UtcNow;
            transaction.PaymentUrlExpiresAt = DateTime.UtcNow.AddMinutes(15); // PayOS expiration window
            transaction.PaymentGatewayOrderCode = approval.OrderCode;
            transaction.UpdatedAt = DateTime.UtcNow;

            // 11. Update original transaction with new payment URL in Content
            if (!string.IsNullOrEmpty(transaction.Content))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(transaction.Content);
                    var root = doc.RootElement;
                    
                    // Create updated content
                    var updatedContent = new Dictionary<string, object>();
                    
                    // Copy existing properties
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Name != "PaymentInfo")
                        {
                            updatedContent[prop.Name] = System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
                        }
                    }
                    
                    // Update PaymentInfo
                    updatedContent["PaymentInfo"] = new
                    {
                        Status = "pending",
                        PaymentUrl = approval.ApprovalUrl,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    
                    transaction.Content = System.Text.Json.JsonSerializer.Serialize(updatedContent);
                    await _transactionRepository.UpdateAsync(transaction, ct);
                }
                catch
                {
                    // If parsing fails, create new content structure
                    var newContent = new
                    {
                        Purpose = purpose,
                        Context = new
                        {
                            UserId = context.UserId,
                            OrgId = context.OrgId,
                            PlanId = context.PlanId,
                            AutoRenew = context.AutoRenew
                        },
                        PaymentInfo = new
                        {
                            Status = "pending",
                            PaymentUrl = approval.ApprovalUrl,
                            LastUpdatedAt = DateTime.UtcNow
                        }
                    };
                    
                    transaction.Content = System.Text.Json.JsonSerializer.Serialize(newContent);
                    await _transactionRepository.UpdateAsync(transaction, ct);
                }
            }
            else
            {
                // Create new content if it doesn't exist
                var newContent = new
                {
                    Purpose = purpose,
                    Context = new
                    {
                        UserId = context.UserId,
                        OrgId = context.OrgId,
                        PlanId = context.PlanId,
                        AutoRenew = context.AutoRenew
                    },
                    PaymentInfo = new
                    {
                        Status = "pending",
                        PaymentUrl = approval.ApprovalUrl,
                        LastUpdatedAt = DateTime.UtcNow
                    }
                };
                
                transaction.Content = System.Text.Json.JsonSerializer.Serialize(newContent);
                await _transactionRepository.UpdateAsync(transaction, ct);
            }

            return Option.Some<RetryPaymentResponse, Error>(new RetryPaymentResponse
            {
                TransactionId = transactionId,
                PaymentUrl = approval.ApprovalUrl,
                Status = "pending",
                Message = "New payment session created",
                ExpiresAt = transaction.PaymentUrlExpiresAt,
                IsNewUrl = true
            });
        }
        catch (Exception ex)
        {
            return Option.None<RetryPaymentResponse, Error>(
                Error.Failure("Payment.RetryFailed", $"Failed to retry payment: {ex.Message}"));
        }
    }

    public async Task<Option<PendingPaymentCheckResponse, Error>> GetPendingPaymentForOrgAsync(
        Guid userId,
        Guid orgId,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Verify user is organization owner
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization == null)
            {
                return Option.None<PendingPaymentCheckResponse, Error>(
                    Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            if (organization.OwnerUserId != userId)
            {
                return Option.None<PendingPaymentCheckResponse, Error>(
                    Error.Forbidden("Organization.Unauthorized",
                        "Only the organization owner can check pending payments"));
            }

            // 2. Query most recent pending transaction
            var pendingTransaction = await _transactionRepository.GetPendingTransactionByOrgAsync(orgId, ct);

            if (pendingTransaction == null)
            {
                return Option.Some<PendingPaymentCheckResponse, Error>(new PendingPaymentCheckResponse
                {
                    HasPending = false,
                    Transaction = null
                });
            }

            // 3. Calculate expiration
            var expiresInMinutes = pendingTransaction.PaymentUrlExpiresAt.HasValue
                ? (int)(pendingTransaction.PaymentUrlExpiresAt.Value - DateTime.UtcNow).TotalMinutes
                : 0;

            // 4. Parse transaction context to get plan ID
            var (purpose, context) = ParseTransactionContext(pendingTransaction);
            if (context?.PlanId == null)
            {
                return Option.None<PendingPaymentCheckResponse, Error>(
                    Error.ValidationError("Transaction.InvalidContext",
                        "Transaction context is missing plan information"));
            }

            // 5. Get plan details
            var plan = await _membershipPlanRepository.GetPlanByIdAsync(context.PlanId.Value, ct);
            if (plan == null)
            {
                return Option.None<PendingPaymentCheckResponse, Error>(
                    Error.NotFound("Plan.NotFound", "Plan not found"));
            }

            return Option.Some<PendingPaymentCheckResponse, Error>(new PendingPaymentCheckResponse
            {
                HasPending = true,
                Transaction = new PendingTransactionDto
                {
                    TransactionId = pendingTransaction.TransactionId.ToString(),
                    PlanId = plan.PlanId,
                    PlanName = plan.PlanName,
                    Amount = pendingTransaction.Amount,
                    Currency = "USD",
                    CreatedAt = pendingTransaction.CreatedAt,
                    PaymentUrl = pendingTransaction.PaymentUrl,
                    ExpiresAt = pendingTransaction.PaymentUrlExpiresAt,
                    ExpiresInMinutes = expiresInMinutes,
                    Description = pendingTransaction.Purpose ?? $"Upgrade to {plan.PlanName}"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending payment for org {OrgId}", orgId);
            return Option.None<PendingPaymentCheckResponse, Error>(
                Error.Failure("Payment.GetPendingFailed", $"Failed to get pending payment: {ex.Message}"));
        }
    }

    public async Task<Option<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>> CancelPaymentWithReasonAsync(
        Guid userId,
        Guid transactionId,
        CancelPaymentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Get transaction
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, ct);
            if (transaction == null)
            {
                return Option.None<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(
                    Error.NotFound("Transaction.NotFound", "Transaction not found"));
            }

            // 2. Verify transaction can be cancelled
            if (transaction.Status?.ToLower() != "pending")
            {
                return Option.None<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(
                    Error.ValidationError("Transaction.CannotCancel",
                        "Only pending transactions can be cancelled"));
            }

            // 3. Parse transaction context and verify ownership
            var (purpose, context) = ParseTransactionContext(transaction);
            if (context?.UserId == null || context.UserId != userId)
            {
                // If not direct owner, check organization ownership
                if (context?.OrgId != null)
                {
                    var organization = await _organizationRepository.GetOrganizationById(context.OrgId.Value);
                    if (organization == null || organization.OwnerUserId != userId)
                    {
                        return Option.None<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(
                            Error.Forbidden("Transaction.Unauthorized",
                                "You do not have permission to cancel this transaction"));
                    }
                }
                else
                {
                    return Option.None<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(
                        Error.Forbidden("Transaction.Unauthorized",
                            "You do not have permission to cancel this transaction"));
                }
            }

            // 4. Cancel transaction
            transaction.Status = "cancelled";
            transaction.CancellationReason = request.Reason;
            transaction.UpdatedAt = DateTime.UtcNow;

            // 5. Update Content field if needed
            if (!string.IsNullOrEmpty(transaction.Content))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(transaction.Content);
                    var root = doc.RootElement;

                    var updatedContent = new Dictionary<string, object>();
                    foreach (var prop in root.EnumerateObject())
                    {
                        updatedContent[prop.Name] = System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
                    }

                    updatedContent["CancellationInfo"] = new
                    {
                        Reason = request.Reason,
                        Notes = request.Notes,
                        CancelledAt = DateTime.UtcNow
                    };

                    transaction.Content = System.Text.Json.JsonSerializer.Serialize(updatedContent);
                }
                catch
                {
                    _logger.LogWarning("Failed to parse transaction content for cancellation update");
                }
            }

            // 6. Save changes
            await _transactionRepository.UpdateAsync(transaction, ct);

            return Option.Some<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(new CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse
            {
                Success = true,
                TransactionId = transactionId.ToString(),
                NewStatus = "cancelled",
                CancellationReason = request.Reason,
                Message = "Transaction cancelled successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel transaction {TransactionId}", transactionId);
            return Option.None<CusomMapOSM_Application.Models.DTOs.Features.Payment.CancelPaymentResponse, Error>(
                Error.Failure("Payment.CancelFailed", $"Failed to cancel payment: {ex.Message}"));
        }
    }

    private async Task<int> AutoCancelConflictingTransactionsAsync(
        Guid orgId,
        Guid excludeTransactionId,
        int paidPlanId,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Get all pending transactions for this organization
            var pendingTransactions = await _transactionRepository
                .GetAllPendingTransactionsByOrgAsync(orgId, ct);

            // 2. Filter out the just-paid transaction
            var conflictingTransactions = pendingTransactions
                .Where(t => t.TransactionId != excludeTransactionId)
                .ToList();

            if (!conflictingTransactions.Any())
                return 0;

            // 3. Get the paid plan details
            var paidPlan = await _membershipPlanRepository.GetPlanByIdAsync(paidPlanId, ct);

            // 4. Cancel all conflicting transactions
            foreach (var transaction in conflictingTransactions)
            {
                transaction.Status = "cancelled";
                transaction.CancellationReason = "superseded";
                transaction.UpdatedAt = DateTime.UtcNow;

                // Update Content field
                if (!string.IsNullOrEmpty(transaction.Content))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(transaction.Content);
                        var root = doc.RootElement;

                        var updatedContent = new Dictionary<string, object>();
                        foreach (var prop in root.EnumerateObject())
                        {
                            updatedContent[prop.Name] = System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
                        }

                        updatedContent["CancellationInfo"] = new
                        {
                            Reason = "superseded",
                            Notes = $"Organization upgraded to {paidPlan?.PlanName}",
                            CancelledAt = DateTime.UtcNow
                        };

                        transaction.Content = System.Text.Json.JsonSerializer.Serialize(updatedContent);
                    }
                    catch
                    {
                        _logger.LogWarning("Failed to update content for auto-cancelled transaction {TransactionId}",
                            transaction.TransactionId);
                    }
                }
            }

            // 5. Save all changes
            await _transactionRepository.UpdateRangeAsync(conflictingTransactions, ct);

            _logger.LogInformation("Auto-cancelled {Count} conflicting transactions for org {OrgId}",
                conflictingTransactions.Count, orgId);

            return conflictingTransactions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-cancel conflicting transactions for org {OrgId}", orgId);
            return 0; // Don't fail the main payment flow
        }
    }

}
