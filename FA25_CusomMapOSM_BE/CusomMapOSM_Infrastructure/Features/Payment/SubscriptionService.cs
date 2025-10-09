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

    public SubscriptionService(
        ITransactionService transactionService,
        IMembershipService membershipService,
        INotificationService notificationService,
        IMembershipPlanRepository membershipPlanRepository,
        IOrganizationRepository organizationRepository,
        ITransactionRepository transactionRepository,
        IMembershipRepository membershipRepository,
        IPaymentGatewayRepository paymentGatewayRepository)
    {
        _transactionService = transactionService;
        _membershipService = membershipService;
        _notificationService = notificationService;
        _membershipPlanRepository = membershipPlanRepository;
        _organizationRepository = organizationRepository;
        _transactionRepository = transactionRepository;
        _membershipRepository = membershipRepository;
        _paymentGatewayRepository = paymentGatewayRepository;
    }

    public async Task<Option<SubscribeResponse, Error>> SubscribeToPlanAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        try
        {
            // Check if user is the owner of the organization
            var userOrgMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(request.UserId, request.OrgId);
            if (userOrgMember is null || userOrgMember.Role?.Name != "Owner")
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
                MembershipId = null
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

            // Calculate pro-rated amount (simplified calculation)
            var proRatedAmount = CalculateProRatedAmount(currentMembership, newPlan);

            // Create payment transaction using existing TransactionService
            var paymentRequest = new ProcessPaymentReq
            {
                UserId = request.UserId,
                OrgId = request.OrgId,
                Total = proRatedAmount,
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
                TransactionId = approval.SessionId, // Use SessionId as transaction identifier
                PaymentUrl = approval.ApprovalUrl,
                Status = "pending", // Payment is pending until confirmed
                Message = "Payment initiated successfully. Complete payment to upgrade your plan.",
                ProRatedAmount = proRatedAmount
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
                AccessToolsGranted = false,
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
                            var upgradeResult = await _membershipService.ChangeSubscriptionPlanAsync(
                                context.UserId.Value, context.OrgId.Value, context.PlanId.Value, context.AutoRenew, ct);
                            response.MembershipUpdated = upgradeResult.HasValue;
                        }
                        break;

                }
            }

            // Grant access tools if membership was updated
            if (response.MembershipUpdated)
            {
                // This would be implemented to grant access tools based on the plan
                response.AccessToolsGranted = true;
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
                        StartDate = membership.StartDate,
                        EndDate = membership.EndDate,
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

    private decimal CalculateProRatedAmount(DomainMembership.Membership currentMembership, DomainMembership.Plan newPlan)
    {
        // Simplified pro-rated calculation
        // In reality, this would be more complex based on billing cycle, remaining time, etc.
        var currentPlanPrice = currentMembership.Plan?.PriceMonthly ?? 0;
        var newPlanPrice = newPlan.PriceMonthly ?? 0;

        // For now, just return the difference
        return Math.Max(0, newPlanPrice - currentPlanPrice);
    }

}
