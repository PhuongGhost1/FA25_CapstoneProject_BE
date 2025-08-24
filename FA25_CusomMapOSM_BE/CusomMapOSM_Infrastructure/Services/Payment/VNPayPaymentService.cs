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

// VNPay API Response Models
public class VNPayPaymentResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public VNPayPaymentData? Data { get; set; }
}

public class VNPayPaymentData
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public class VNPayPaymentDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public VNPayPaymentDetailData? Data { get; set; }
}

public class VNPayPaymentDetailData
{
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
}

public class VNPayPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public VNPayPaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://sandbox.vnpayment.vn");
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(decimal amount, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        try
        {
            var orderId = GenerateOrderId();
            var amountInVND = (long)(amount * 24500); // Convert to VND and ensure it's an integer

            Console.WriteLine($"=== VNPay Payment Request ===");
            Console.WriteLine($"Order ID: {orderId}");
            Console.WriteLine($"Amount: {amountInVND}");
            Console.WriteLine($"Return URL: {returnUrl}");
            Console.WriteLine($"Cancel URL: {cancelUrl}");
            Console.WriteLine($"=== End VNPay Payment Request ===");

            // Validate VNPay credentials
            if (string.IsNullOrEmpty(VnPayConstant.VNPAY_TMN_CODE) ||
                string.IsNullOrEmpty(VnPayConstant.VNPAY_HASH_SECRET) ||
                string.IsNullOrEmpty(VnPayConstant.VNPAY_URL))
            {
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.VNPay.InvalidCredentials", "VNPay credentials are missing or invalid", ErrorCustom.ErrorType.Validation));
            }

            // Create VNPay payment URL
            var paymentUrl = CreateVNPayPaymentUrl(orderId, amountInVND, returnUrl, cancelUrl);

            Console.WriteLine($"=== VNPay Payment URL ===");
            Console.WriteLine($"Payment URL: {paymentUrl}");
            Console.WriteLine($"=== End VNPay Payment URL ===");

            return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(new ApprovalUrlResponse
            {
                ApprovalUrl = paymentUrl,
                PaymentGateway = PaymentGatewayEnum.VNPay,
                SessionId = orderId,
                QrCode = "", // VNPay QR code is generated on their side
                OrderCode = orderId
            });
        }
        catch (Exception ex)
        {
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.VNPay.Exception", $"Exception occurred: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<ConfirmPaymentResponse, ErrorCustom.Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(req.OrderCode))
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.VNPay.MissingOrderCode", "Order code is required for VNPay", ErrorCustom.ErrorType.Validation));
            }

            // For VNPay, we need to verify the payment using the transaction ID
            // This would typically be done by checking the payment status with VNPay API
            // For now, we'll return success if we have the required data

            Console.WriteLine($"=== VNPay Payment Confirmation ===");
            Console.WriteLine($"Order Code: {req.OrderCode}");
            Console.WriteLine($"Payment ID: {req.PaymentId}");
            Console.WriteLine($"Signature: {req.Signature}");
            Console.WriteLine($"=== End VNPay Payment Confirmation ===");

            return Option.Some<ConfirmPaymentResponse, ErrorCustom.Error>(new ConfirmPaymentResponse
            {
                PaymentId = req.PaymentId,
                PaymentGateway = PaymentGatewayEnum.VNPay,
                OrderCode = req.OrderCode,
                Signature = req.Signature
            });
        }
        catch (Exception ex)
        {
            return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.VNPay.Exception", $"Exception occurred: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(req.OrderCode))
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.VNPay.MissingOrderCode", "Order code is required for VNPay", ErrorCustom.ErrorType.Validation));
            }

            Console.WriteLine($"=== VNPay Payment Cancellation ===");
            Console.WriteLine($"Order Code: {req.OrderCode}");
            Console.WriteLine($"Payment ID: {req.PaymentId}");
            Console.WriteLine($"=== End VNPay Payment Cancellation ===");

            // For VNPay, cancellation is typically handled by the user not completing the payment
            return Option.Some<CancelPaymentResponse, ErrorCustom.Error>(new CancelPaymentResponse(
                "cancelled",
                PaymentGatewayEnum.VNPay.ToString()
            ));
        }
        catch (Exception ex)
        {
            return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.VNPay.CancelFailed", $"Failed to cancel payment: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    private string GenerateOrderId()
    {
        return $"VNPAY_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    private string CreateVNPayPaymentUrl(string orderId, long amount, string returnUrl, string cancelUrl)
    {
        var vnpUrl = VnPayConstant.VNPAY_URL;
        var vnpReturnUrl = returnUrl;
        var vnpTmnCode = VnPayConstant.VNPAY_TMN_CODE;
        var vnpHashSecret = VnPayConstant.VNPAY_HASH_SECRET;

        var vnpParams = new Dictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnpTmnCode,
            ["vnp_Amount"] = (amount * 100).ToString(), // VNPay expects amount in smallest currency unit (VND * 100)
            ["vnp_CurrCode"] = "VND",
            ["vnp_BankCode"] = "", // Leave empty for all banks
            ["vnp_TxnRef"] = orderId,
            ["vnp_OrderInfo"] = "Payment for CustomMapOSM service",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = vnpReturnUrl,
            ["vnp_IpAddr"] = "127.0.0.1", // Should be replaced with actual client IP
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss")
        };

        // Sort parameters by key
        var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();

        // Create query string
        var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        // Create signature
        var signData = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));
        var signature = GenerateVNPaySignature(signData, vnpHashSecret);

        // Add signature to query string
        var finalQueryString = $"{queryString}&vnp_SecureHash={signature}";

        Console.WriteLine($"=== VNPay Signature Generation ===");
        Console.WriteLine($"Sign Data: {signData}");
        Console.WriteLine($"Hash Secret: {vnpHashSecret}");
        Console.WriteLine($"Generated Signature: {signature}");
        Console.WriteLine($"=== End VNPay Signature Generation ===");

        return $"{vnpUrl}?{finalQueryString}";
    }

    private string GenerateVNPaySignature(string signData, string hashSecret)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
        return Convert.ToHexString(hash).ToLower();
    }
}
