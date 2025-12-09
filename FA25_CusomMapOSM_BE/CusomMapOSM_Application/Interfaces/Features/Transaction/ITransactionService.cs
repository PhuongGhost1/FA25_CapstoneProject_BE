using CusomMapOSM_Domain.Entities.Transactions;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Application.Interfaces.Services.Payment;

namespace CusomMapOSM_Application.Interfaces.Features.Transaction;

public interface ITransactionService
{
    Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> ProcessPaymentAsync(ProcessPaymentReq request, CancellationToken ct);
    Task<Option<object, ErrorCustom.Error>> ConfirmPaymentWithContextAsync(ConfirmPaymentWithContextReq req, CancellationToken ct);

    IPaymentService GetPaymentService(PaymentGatewayEnum gateway);

    Task<Option<Transactions, ErrorCustom.Error>> CreateTransactionRecordAsync(Guid paymentGatewayId, decimal amount, string purpose, Guid? membershipId, int? exportId, string status, CancellationToken ct);
    Task<Option<Transactions, ErrorCustom.Error>> UpdateTransactionStatusAsync(Guid transactionId, string status, CancellationToken ct);
    Task<Option<Transactions, ErrorCustom.Error>> GetTransactionAsync(Guid transactionId, CancellationToken ct);
    Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentWithContextAsync(CancelPaymentWithContextReq req, CancellationToken ct);
    Task<Option<Transactions, ErrorCustom.Error>> UpdateTransactionGatewayInfoAsync(Guid transactionId, string gatewayReference, CancellationToken ct);
    Task<Option<object, ErrorCustom.Error>> HandleWebhookAsync(PaymentGatewayEnum gateway, string gatewayReference, string? orderCode = null, CancellationToken ct = default);
}