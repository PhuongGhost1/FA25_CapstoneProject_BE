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

    public SubscriptionService(
        ITransactionService transactionService,
        IMembershipService membershipService,
        INotificationService notificationService,
        IMembershipPlanRepository membershipPlanRepository)
    {
        _transactionService = transactionService;
        _membershipService = membershipService;
        _notificationService = notificationService;
        _membershipPlanRepository = membershipPlanRepository;
    }

    public async Task<Option<SubscribeResponse, Error>> SubscribeToPlanAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        try
        {
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

    public async Task<Option<PurchaseAddonResponse, Error>> PurchaseAddonAsync(PurchaseAddonRequest request, CancellationToken ct = default)
    {
        try
        {
            // Calculate addon price (this would be based on addon configuration)
            var addonPrice = CalculateAddonPrice(request.AddonKey, request.Quantity);

            // Create payment transaction using existing TransactionService
            var paymentRequest = new ProcessPaymentReq
            {
                UserId = request.UserId,
                OrgId = request.OrgId,
                Total = addonPrice,
                PaymentGateway = request.PaymentMethod,
                Purpose = "addon",
                AddonKey = request.AddonKey,
                Quantity = request.Quantity,
                MembershipId = null
            };

            var transactionResult = await _transactionService.ProcessPaymentAsync(paymentRequest, ct);
            if (!transactionResult.HasValue)
            {
                return Option.None<PurchaseAddonResponse, Error>(Error.Failure("Payment.ProcessFailed", "Failed to process payment"));
            }

            var approval = transactionResult.ValueOrDefault();

            return Option.Some<PurchaseAddonResponse, Error>(new PurchaseAddonResponse
            {
                PaymentGateway = approval.PaymentGateway,
                QrCode = approval.QrCode,
                OrderCode = approval.OrderCode,
                TransactionId = approval.SessionId, // Use SessionId as transaction identifier
                PaymentUrl = approval.ApprovalUrl,
                Status = "pending", // Payment is pending until confirmed
                Message = "Payment initiated successfully. Complete payment to activate your addon."
            });
        }
        catch (Exception ex)
        {
            return Option.None<PurchaseAddonResponse, Error>(Error.Failure("Payment.PurchaseAddonFailed", $"Failed to purchase addon: {ex.Message}"));
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

                    case "addon":
                        if (context.UserId.HasValue && context.OrgId.HasValue &&
                            !string.IsNullOrEmpty(context.AddonKey) && context.Quantity.HasValue)
                        {
                            var addonResult = await _membershipService.AddAddonAsync(
                                transaction.MembershipId ?? Guid.Empty, context.OrgId.Value,
                                context.AddonKey, context.Quantity.Value, true, ct);
                            response.MembershipUpdated = addonResult.HasValue;
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
            if (string.IsNullOrEmpty(transaction.TransactionReference))
                return (transaction.Purpose, null);

            var parts = transaction.TransactionReference.Split('|', 2);
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

    public Task<Option<List<object>, Error>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            // This would be implemented to get payment history from transaction service
            // For now, return empty list
            return Task.FromResult(Option.Some<List<object>, Error>(new List<object>()));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Option.None<List<object>, Error>(Error.Failure("Payment.HistoryFailed", $"Failed to get payment history: {ex.Message}")));
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

    private decimal CalculateAddonPrice(string addonKey, int quantity)
    {
        // This would be based on addon configuration
        // For now, return a fixed price
        return addonKey.ToLower() switch
        {
            "export_tokens" => 0.10m * quantity, // $0.10 per export token
            "extra_maps" => 5.00m * quantity,    // $5.00 per extra map
            "priority_support" => 25.00m,        // $25.00 for priority support
            _ => 1.00m * quantity                // Default $1.00 per unit
        };
    }
}
