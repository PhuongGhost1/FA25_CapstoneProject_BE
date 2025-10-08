using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Payment;

public interface ISubscriptionService
{
    // Subscription management (business logic layer)
    Task<Option<SubscribeResponse, Error>> SubscribeToPlanAsync(SubscribeRequest request, CancellationToken ct = default);
    Task<Option<UpgradeResponse, Error>> UpgradePlanAsync(UpgradeRequest request, CancellationToken ct = default);

    // Process successful payment and update membership
    Task<Option<PaymentConfirmationResponse, Error>> ProcessSuccessfulPaymentAsync(Guid transactionId, CancellationToken ct = default);

    // Payment history
    Task<Option<List<object>, Error>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
