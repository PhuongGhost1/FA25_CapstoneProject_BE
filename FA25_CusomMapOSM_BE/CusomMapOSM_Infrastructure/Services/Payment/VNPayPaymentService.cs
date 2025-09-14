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
        // Create a simple request for backward compatibility
        var simpleRequest = new ProcessPaymentReq
        {
            Total = amount,
            Purpose = "membership", // Default purpose
            PaymentGateway = PaymentGatewayEnum.VNPay
        };

        return await CreateCheckoutAsync(simpleRequest, returnUrl, cancelUrl, ct);
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(ProcessPaymentReq request, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        try
        {
            var orderId = GenerateOrderId();
            var amountInVND = (long)(request.Total * 24500); // Convert to VND and ensure it's an integer

            Console.WriteLine($"=== VNPay Payment Request ===");
            Console.WriteLine($"Order ID: {orderId}");
            Console.WriteLine($"Purpose: {request.Purpose}");
            Console.WriteLine($"Amount: {amountInVND}");
            if (request.Purpose?.ToLower() == "addon" && !string.IsNullOrEmpty(request.AddonKey))
            {
                Console.WriteLine($"Addon: {request.AddonKey} (Qty: {request.Quantity ?? 1})");
            }
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

            Console.WriteLine($"=== VNPay Payment Confirmation ===");
            Console.WriteLine($"Order Code: {req.OrderCode}");
            Console.WriteLine($"Payment ID: {req.PaymentId}");
            Console.WriteLine($"Signature: {req.Signature}");
            Console.WriteLine($"=== End VNPay Payment Confirmation ===");

            // VNPay payment verification is typically done in the ReturnURL handler
            // This method is called after VNPay redirects back to our application
            // The verification should check the signature and response code from VNPay

            // For VNPay, the actual verification happens when VNPay calls our ReturnURL
            // with parameters like vnp_ResponseCode, vnp_TransactionNo, vnp_SecureHash, etc.
            // This method is more of a placeholder for the application flow

            Console.WriteLine($"=== VNPay Payment Verification ===");
            Console.WriteLine($"Note: VNPay verification is handled in ReturnURL endpoint with vnp_ResponseCode");
            Console.WriteLine($"Order Code: {req.OrderCode}");
            Console.WriteLine($"Payment ID: {req.PaymentId}");
            Console.WriteLine($"=== End VNPay Payment Verification ===");

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

        // Create parameters dictionary (sorted automatically by key)
        var vnpParams = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnpTmnCode,
            ["vnp_Amount"] = (amount * 100).ToString(), // VNPay expects amount in smallest currency unit (VND * 100)
            ["vnp_CurrCode"] = "VND",
            ["vnp_BankCode"] = "", // Leave empty for all banks - this is required parameter
            ["vnp_TxnRef"] = orderId,
            ["vnp_OrderInfo"] = "Payment for CustomMapOSM service",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = vnpReturnUrl,
            ["vnp_IpAddr"] = "127.0.0.1", // Should be replaced with actual client IP
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss")
        };

        // Build signature data exactly like VNPay official library (raw values, not URL encoded)
        var signData = new StringBuilder();
        foreach (var kvp in vnpParams)
        {
            signData.Append($"{kvp.Key}={kvp.Value}&");
        }

        // Remove last '&'
        if (signData.Length > 0)
            signData.Length--;

        // Generate signature from raw data (not URL encoded)
        var signature = GenerateVNPaySignature(signData.ToString(), vnpHashSecret);

        Console.WriteLine($"=== VNPay Signature Generation ===");
        Console.WriteLine($"Sign Data: {signData}");
        Console.WriteLine($"Hash Secret: {vnpHashSecret}");
        Console.WriteLine($"Generated Signature: {signature}");
        Console.WriteLine($"=== End VNPay Signature Generation ===");

        // Add signature to parameters
        vnpParams["vnp_SecureHash"] = signature;

        // Build final URL with proper URL encoding (like qs.stringify with encode: false)
        var queryParams = new List<string>();
        foreach (var kvp in vnpParams)
        {
            // Use proper URL encoding that matches VNPay expectations
            var encodedKey = Uri.EscapeDataString(kvp.Key);
            var encodedValue = Uri.EscapeDataString(kvp.Value);
            queryParams.Add($"{encodedKey}={encodedValue}");
        }

        var finalUrl = $"{vnpUrl}?{string.Join("&", queryParams)}";

        Console.WriteLine($"=== VNPay Payment URL ===");
        Console.WriteLine($"Payment URL: {finalUrl}");
        Console.WriteLine($"=== End VNPay Payment URL ===");

        return finalUrl;
    }

    private string GenerateVNPaySignature(string signData, string hashSecret)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
        return Convert.ToHexString(hash).ToLower();
    }

    /// <summary>
    /// Verifies VNPay signature according to official documentation
    /// This method should be used in the ReturnURL handler to verify payment
    /// </summary>
    /// <param name="parameters">Dictionary of VNPay parameters (excluding vnp_SecureHash and vnp_SecureHashType)</param>
    /// <param name="receivedSignature">The signature received from VNPay</param>
    /// <param name="hashSecret">VNPay hash secret</param>
    /// <returns>True if signature is valid</returns>
    public static bool VerifyVNPaySignature(Dictionary<string, string> parameters, string receivedSignature, string hashSecret)
    {
        try
        {
            // Create a copy to avoid modifying the original
            var paramsCopy = new Dictionary<string, string>(parameters);

            // Remove signature parameters
            paramsCopy.Remove("vnp_SecureHash");
            paramsCopy.Remove("vnp_SecureHashType");

            // Sort parameters by key (alphabetical order as required by VNPay)
            var sortedParams = new SortedDictionary<string, string>(paramsCopy);

            // Build signature data exactly like VNPay official library (raw values, not URL encoded)
            var signData = new StringBuilder();
            foreach (var kvp in sortedParams)
            {
                signData.Append($"{kvp.Key}={kvp.Value}&");
            }

            // Remove last '&'
            if (signData.Length > 0)
                signData.Length--;

            // Generate signature using HMAC-SHA512
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData.ToString()));
            var generatedSignature = Convert.ToHexString(hash).ToLower();

            Console.WriteLine($"=== VNPay Signature Verification ===");
            Console.WriteLine($"Sign Data: {signData}");
            Console.WriteLine($"Received Signature: {receivedSignature}");
            Console.WriteLine($"Generated Signature: {generatedSignature}");
            Console.WriteLine($"Signatures Match: {generatedSignature == receivedSignature}");
            Console.WriteLine($"=== End VNPay Signature Verification ===");

            return generatedSignature == receivedSignature;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== VNPay Signature Verification Error ===");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"=== End VNPay Signature Verification Error ===");
            return false;
        }
    }

    /// <summary>
    /// Checks if VNPay response code indicates successful payment
    /// According to VNPay documentation: 00 = Success, 07 = Success but suspicious
    /// </summary>
    /// <param name="responseCode">VNPay response code</param>
    /// <returns>True if payment was successful</returns>
    public static bool IsVNPayPaymentSuccessful(string responseCode)
    {
        return responseCode == "00" || responseCode == "07";
    }
}
