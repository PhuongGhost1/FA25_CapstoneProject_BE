using System;
using CusomMapOSM_Domain.Entities.Transactions.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Transaction;

public record CreateTransactionRequest(string GatewayName, decimal Amount, string Purpose, Guid? MembershipId, int? ExportId);
public record CreateTransactionResponse(Guid TransactionId, string Status, string GatewayName);

public record CancelPaymentWithContextReq(PaymentGatewayEnum PaymentGateway, string PaymentId, string PayerId, string Token, string PaymentIntentId, string ClientSecret, string OrderCode, string Signature, Guid TransactionId);

public record CancelPaymentResponse(string Status, string GatewayName);