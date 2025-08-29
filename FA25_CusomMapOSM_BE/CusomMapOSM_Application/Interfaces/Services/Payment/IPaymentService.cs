using CusomMapOSM_Application.Models.DTOs.Services;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;

namespace CusomMapOSM_Application.Interfaces.Services.Payment;

public interface IPaymentService
{
    Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(decimal amount, string returnUrl, string cancelUrl, CancellationToken ct);

    // New overload for multi-item support
    Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(ProcessPaymentReq request, string returnUrl, string cancelUrl, CancellationToken ct);

    Task<Option<ConfirmPaymentResponse, ErrorCustom.Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct);

    Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct);
}
