using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Commons.Constant;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Services.Payment;

public class PayOSPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public PayOSPaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn");
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(decimal amount, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        try
        {
            var orderCode = GenerateOrderCode();
            var amountInVND = amount;

            var requestData = new
            {
                orderCode = orderCode,
                amount = amountInVND,
                description = "Payment for CustomMapOSM service",
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                signature = GenerateSignature(orderCode, amountInVND.ToString(), PayOsConstant.PAYOS_CHECKSUM_KEY)
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add authentication headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", PayOsConstant.PAYOS_CLIENT_ID);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", PayOsConstant.PAYOS_API_KEY);

            var response = await _httpClient.PostAsync("/v2/payment-requests", content, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.CreateFailed", $"Failed to create payment: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            var paymentResponse = JsonSerializer.Deserialize<PayOSPaymentResponse>(responseContent);

            if (paymentResponse?.Data == null)
            {
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidResponse", "Invalid response from PayOS", ErrorCustom.ErrorType.Failure));
            }

            return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(new ApprovalUrlResponse
            {
                ApprovalUrl = paymentResponse.Data.CheckoutUrl,
                PaymentGateway = PaymentGatewayEnum.PayOS,
                SessionId = paymentResponse.Data.PaymentLinkId,
                QrCode = paymentResponse.Data.QrCode,
                OrderCode = orderCode
            });
        }
        catch (Exception ex)
        {
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.PayOS.Exception", $"Exception occurred: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<ConfirmPaymentResponse, ErrorCustom.Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(req.OrderCode))
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.MissingOrderCode", "Order code is required for PayOS", ErrorCustom.ErrorType.Validation));
            }

            // Verify signature
            var expectedSignature = GenerateSignature(req.OrderCode, req.PaymentId, PayOsConstant.PAYOS_CHECKSUM_KEY);
            if (req.Signature != expectedSignature)
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidSignature", "Invalid signature", ErrorCustom.ErrorType.Validation));
            }

            // Get payment details from PayOS
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", PayOsConstant.PAYOS_CLIENT_ID);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", PayOsConstant.PAYOS_API_KEY);

            var response = await _httpClient.GetAsync($"/v2/payment-requests/{req.PaymentId}", ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.GetFailed", $"Failed to get payment details: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            var paymentDetails = JsonSerializer.Deserialize<PayOSPaymentDetails>(responseContent);

            if (paymentDetails?.Data == null)
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidResponse", "Invalid response from PayOS", ErrorCustom.ErrorType.Failure));
            }

            // Check if payment is successful
            if (paymentDetails.Data.Status != "PAID")
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.NotPaid", "Payment is not completed", ErrorCustom.ErrorType.Validation));
            }

            return Option.Some<ConfirmPaymentResponse, ErrorCustom.Error>(new ConfirmPaymentResponse
            {
                PaymentId = req.PaymentId,
                PaymentGateway = PaymentGatewayEnum.PayOS,
                OrderCode = req.OrderCode,
                Signature = req.Signature
            });
        }
        catch (Exception ex)
        {
            return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.PayOS.Exception", $"Exception occurred: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        try
        {
            // For PayOS, we need to check the payment status first
            if (string.IsNullOrEmpty(req.OrderCode))
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.MissingOrderCode", "Order code is required for PayOS", ErrorCustom.ErrorType.Validation));
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", PayOsConstant.PAYOS_CLIENT_ID);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", PayOsConstant.PAYOS_API_KEY);

            var response = await _httpClient.GetAsync($"/v2/payment-requests/{req.PaymentId}", ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.GetFailed", $"Failed to get payment details: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            var paymentDetails = JsonSerializer.Deserialize<PayOSPaymentDetails>(responseContent);

            if (paymentDetails?.Data == null)
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidResponse", "Invalid response from PayOS", ErrorCustom.ErrorType.Failure));
            }

            // Check if payment is in a cancellable state
            if (paymentDetails.Data.Status == "PAID")
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.AlreadyPaid", "Payment has already been completed and cannot be cancelled", ErrorCustom.ErrorType.Validation));
            }

            // For PayOS, cancellation is typically handled by the user not completing the payment
            // We can mark it as cancelled in our system
            return Option.Some<CancelPaymentResponse, ErrorCustom.Error>(new CancelPaymentResponse(
                "cancelled",
                PaymentGatewayEnum.PayOS.ToString()
            ));
        }
        catch (Exception ex)
        {
            return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.PayOS.CancelFailed", $"Failed to cancel payment: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    private string GenerateOrderCode()
    {
        return $"ORDER_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    private string GenerateSignature(string orderCode, string amount, string checksumKey)
    {
        var data = $"{orderCode}{amount}{checksumKey}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
