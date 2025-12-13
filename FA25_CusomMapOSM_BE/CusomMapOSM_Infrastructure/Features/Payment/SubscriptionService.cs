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

    public SubscriptionService(
        ITransactionService transactionService,
        IMembershipService membershipService,
        INotificationService notificationService,
        IMembershipPlanRepository membershipPlanRepository,
        IOrganizationRepository organizationRepository,
        ITransactionRepository transactionRepository,
        IMembershipRepository membershipRepository,
        IPaymentGatewayRepository paymentGatewayRepository,
        IProrationService prorationService)
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

                var paymentHistoryItem = new
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    Purpose = transaction.Purpose,
                    TransactionDate = transaction.TransactionDate,
                    CreatedAt = transaction.CreatedAt,
                    TransactionReference = transaction.TransactionReference,
                    PaymentGateway = paymentGateway != null ? new
                    {
                        GatewayId = paymentGateway.GatewayId,
                        Name = paymentGateway.Name
                    } : null,
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


}
