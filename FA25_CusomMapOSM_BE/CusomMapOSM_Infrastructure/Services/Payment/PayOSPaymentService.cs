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
using System.Text.Json.Serialization;

// PayOS API Response Models
public class PayOSPaymentResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public PayOSPaymentData? Data { get; set; }
}

public class PayOSPaymentData
{
    [JsonPropertyName("bin")]
    public string Bin { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("paymentLinkId")]
    public string PaymentLinkId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("checkoutUrl")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrCode")]
    public string QrCode { get; set; } = string.Empty;
}

public class PayOSPaymentDetails
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public PayOSPaymentDetailData? Data { get; set; }
}

public class PayOSPaymentDetailData
{
    [JsonPropertyName("bin")]
    public string Bin { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("paymentLinkId")]
    public string PaymentLinkId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("checkoutUrl")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrCode")]
    public string QrCode { get; set; } = string.Empty;
}

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
        // Create a simple request for backward compatibility
        var simpleRequest = new ProcessPaymentReq
        {
            Total = amount,
            Purpose = "membership", // Default purpose
            PaymentGateway = PaymentGatewayEnum.PayOS
        };

        return await CreateCheckoutAsync(simpleRequest, returnUrl, cancelUrl, ct);
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(ProcessPaymentReq request, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"=== PayOS CreateCheckoutAsync START ===");
            Console.WriteLine($"Request Total: {request.Total}");
            Console.WriteLine($"Request Purpose: {request.Purpose}");
            Console.WriteLine($"Return URL: {returnUrl}");
            Console.WriteLine($"Cancel URL: {cancelUrl}");

            var orderCode = GenerateOrderCode();
            var amountInVND = (long)(request.Total * 24500); // Convert to VND and ensure it's an integer

            Console.WriteLine($"Generated Order Code: {orderCode}");
            Console.WriteLine($"Amount in VND: {amountInVND}");

            // Determine description and items based on purpose
            string description;
            object[] items;

            if (request.Purpose?.ToLower() == "membership")
            {
                // Membership purchase (default)
                description = "CustomMapOSM Membership";
                items = new[]
                {
                    new
                    {
                        name = "CustomMapOSM Membership",
                        quantity = 1,
                        price = amountInVND
                    }
                };
            }
            else
            {
                // Default case for other purposes
                description = "CustomMapOSM Service";
                items = new[]
                {
                    new
                    {
                        name = "CustomMapOSM Service",
                        quantity = 1,
                        price = amountInVND
                    }
                };
            }

            // Ensure description fits PayOS 25-character limit
            if (description.Length > 25)
            {
                description = description.Substring(0, 22) + "...";
            }

            var requestData = new
            {
                orderCode = orderCode,
                amount = amountInVND,
                description = description,
                items = items,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl
            };

            // Generate signature
            var signature = GeneratePayOSSignatureComprehensive(orderCode, amountInVND, description, returnUrl, cancelUrl, PayOsConstant.PAYOS_CHECKSUM_KEY);

            // Add signature to request data
            var requestWithSignature = new
            {
                orderCode = orderCode,
                amount = amountInVND,
                description = description,
                items = items,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                signature = signature
            };

            Console.WriteLine($"=== PayOS Official Request with Items ===");
            Console.WriteLine($"Order Code: {orderCode}");
            Console.WriteLine($"Amount: {amountInVND}");
            Console.WriteLine($"Purpose: {request.Purpose}");
            Console.WriteLine($"Description: {description}");
            Console.WriteLine($"Items: {items.Length} item(s)");
            foreach (var item in items)
            {
                // Use reflection to get the properties instead of casting to IDictionary
                var itemType = item.GetType();
                var name = itemType.GetProperty("name")?.GetValue(item)?.ToString() ?? "Unknown";
                var quantity = itemType.GetProperty("quantity")?.GetValue(item)?.ToString() ?? "0";
                var price = itemType.GetProperty("price")?.GetValue(item)?.ToString() ?? "0";
                Console.WriteLine($"  - {name} (Qty: {quantity}, Price: {price})");
            }
            Console.WriteLine($"Return URL: {returnUrl}");
            Console.WriteLine($"Cancel URL: {cancelUrl}");
            Console.WriteLine($"Signature: {signature}");
            Console.WriteLine($"=== End PayOS Official Request with Items ===");

            // Validate PayOS credentials
            Console.WriteLine($"=== PayOS Credentials Validation ===");
            Console.WriteLine($"PAYOS_CLIENT_ID: {PayOsConstant.PAYOS_CLIENT_ID ?? "NULL"}");
            Console.WriteLine($"PAYOS_API_KEY: {PayOsConstant.PAYOS_API_KEY ?? "NULL"}");
            Console.WriteLine($"PAYOS_CHECKSUM_KEY: {PayOsConstant.PAYOS_CHECKSUM_KEY ?? "NULL"}");

            if (string.IsNullOrEmpty(PayOsConstant.PAYOS_CLIENT_ID) ||
                string.IsNullOrEmpty(PayOsConstant.PAYOS_API_KEY) ||
                string.IsNullOrEmpty(PayOsConstant.PAYOS_CHECKSUM_KEY))
            {
                Console.WriteLine($"=== PayOS Credentials Validation FAILED ===");
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidCredentials", "PayOS credentials are missing or invalid", ErrorCustom.ErrorType.Validation));
            }

            Console.WriteLine($"=== PayOS Credentials Validation PASSED ===");

            var json = JsonSerializer.Serialize(requestWithSignature);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add authentication headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", PayOsConstant.PAYOS_CLIENT_ID);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", PayOsConstant.PAYOS_API_KEY);

            Console.WriteLine($"=== PayOS Request Details ===");
            Console.WriteLine($"URL: {_httpClient.BaseAddress}/v2/payment-requests");
            Console.WriteLine($"Headers: x-client-id={PayOsConstant.PAYOS_CLIENT_ID}, x-api-key={PayOsConstant.PAYOS_API_KEY}");
            Console.WriteLine($"Request Body: {json}");
            Console.WriteLine($"=== End PayOS Request Details ===");

            var response = await _httpClient.PostAsync("/v2/payment-requests", content, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            Console.WriteLine($"=== PayOS Response ===");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content: {responseContent}");
            Console.WriteLine($"=== End PayOS Response ===");

            if (!response.IsSuccessStatusCode)
            {
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.CreateFailed", $"Failed to create payment: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            PayOSPaymentResponse? paymentResponse;
            try
            {
                Console.WriteLine($"=== PayOS JSON Deserialization ===");
                Console.WriteLine($"Attempting to deserialize: {responseContent}");

                paymentResponse = JsonSerializer.Deserialize<PayOSPaymentResponse>(responseContent);

                Console.WriteLine($"Deserialization successful: {paymentResponse != null}");
                if (paymentResponse != null)
                {
                    Console.WriteLine($"Code: {paymentResponse.Code}");
                    Console.WriteLine($"Desc: {paymentResponse.Desc}");
                    Console.WriteLine($"Data: {paymentResponse.Data != null}");
                    if (paymentResponse.Data != null)
                    {
                        Console.WriteLine($"PaymentLinkId: {paymentResponse.Data.PaymentLinkId}");
                        Console.WriteLine($"CheckoutUrl: {paymentResponse.Data.CheckoutUrl}");
                        Console.WriteLine($"QrCode: {paymentResponse.Data.QrCode}");
                    }
                }
                Console.WriteLine($"=== End PayOS JSON Deserialization ===");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"=== PayOS JSON Deserialization ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Response: {responseContent}");
                Console.WriteLine($"=== End PayOS JSON Deserialization ERROR ===");

                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.JsonParseError", $"Failed to parse PayOS response: {ex.Message}. Response: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            if (paymentResponse?.Data == null)
            {
                return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidResponse", $"Invalid response from PayOS: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            Console.WriteLine($"=== PayOS Creating ApprovalUrlResponse ===");
            Console.WriteLine($"CheckoutUrl: {paymentResponse.Data.CheckoutUrl}");
            Console.WriteLine($"PaymentLinkId: {paymentResponse.Data.PaymentLinkId}");
            Console.WriteLine($"QrCode: {paymentResponse.Data.QrCode}");
            Console.WriteLine($"OrderCode: {orderCode}");
            Console.WriteLine($"=== End PayOS Creating ApprovalUrlResponse ===");

            return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(new ApprovalUrlResponse
            {
                ApprovalUrl = paymentResponse.Data.CheckoutUrl,
                PaymentGateway = PaymentGatewayEnum.PayOS,
                SessionId = paymentResponse.Data.PaymentLinkId,
                QrCode = paymentResponse.Data.QrCode,
                OrderCode = orderCode.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== PayOS CreateCheckoutAsync EXCEPTION ===");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Exception Message: {ex.Message}");
            Console.WriteLine($"Exception Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"=== End PayOS CreateCheckoutAsync EXCEPTION ===");

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

            // Get payment details from PayOS
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", PayOsConstant.PAYOS_CLIENT_ID);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", PayOsConstant.PAYOS_API_KEY);

            var response = await _httpClient.GetAsync($"/v2/payment-requests/{req.PaymentId}", ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            Console.WriteLine($"=== PayOS Confirm Payment Response ===");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content: {responseContent}");
            Console.WriteLine($"=== End PayOS Confirm Payment Response ===");

            if (!response.IsSuccessStatusCode)
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.GetFailed", $"Failed to get payment details: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            PayOSPaymentDetails? paymentDetails;
            try
            {
                Console.WriteLine($"=== PayOS Confirm Payment JSON Deserialization ===");
                Console.WriteLine($"Attempting to deserialize: {responseContent}");

                paymentDetails = JsonSerializer.Deserialize<PayOSPaymentDetails>(responseContent);

                Console.WriteLine($"Deserialization successful: {paymentDetails != null}");
                if (paymentDetails != null)
                {
                    Console.WriteLine($"Code: {paymentDetails.Code}");
                    Console.WriteLine($"Desc: {paymentDetails.Desc}");
                    Console.WriteLine($"Data: {paymentDetails.Data != null}");
                    if (paymentDetails.Data != null)
                    {
                        Console.WriteLine($"Status: {paymentDetails.Data.Status}");
                        Console.WriteLine($"Amount: {paymentDetails.Data.Amount}");
                        Console.WriteLine($"OrderCode: {paymentDetails.Data.OrderCode}");
                    }
                }
                Console.WriteLine($"=== End PayOS Confirm Payment JSON Deserialization ===");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"=== PayOS Confirm Payment JSON Deserialization ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Response: {responseContent}");
                Console.WriteLine($"=== End PayOS Confirm Payment JSON Deserialization ERROR ===");

                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.JsonParseError", $"Failed to parse PayOS response: {ex.Message}. Response: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            if (paymentDetails?.Data == null)
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.InvalidResponse", $"Invalid response from PayOS: {responseContent}", ErrorCustom.ErrorType.Failure));
            }

            // Check if payment is successful
            Console.WriteLine($"=== PayOS Payment Status Check ===");
            Console.WriteLine($"Current Status: '{paymentDetails.Data.Status}'");
            Console.WriteLine($"Expected Status: 'PAID'");
            Console.WriteLine($"Status Match: {paymentDetails.Data.Status == "PAID"}");
            Console.WriteLine($"=== End PayOS Payment Status Check ===");

            if (paymentDetails.Data.Status != "PAID")
            {
                return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.PayOS.NotPaid", $"Payment is not completed. Current status: '{paymentDetails.Data.Status}'. Expected status: 'PAID'. Please complete the payment on PayOS gateway first.", ErrorCustom.ErrorType.Validation));
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

    private long GenerateOrderCode()
    {
        // Generate a numeric order code that fits within PayOS constraints
        // Use timestamp + random number to ensure uniqueness
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = new Random().Next(1000, 9999);
        var orderCode = timestamp * 10000 + random;

        // Ensure it doesn't exceed PayOS limit (9007199254740991)
        if (orderCode > 9007199254740991)
        {
            orderCode = orderCode % 9007199254740991;
        }

        return orderCode;
    }



    // PayOS signature generation according to official documentation
    // Format: SHA256(amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl + checksumKey)
    private string GeneratePayOSSignatureComprehensive(long orderCode, long amount, string description, string returnUrl, string cancelUrl, string checksumKey)
    {
        Console.WriteLine($"=== PayOS Official Signature Generation ===");

        // Create parameters dictionary and sort alphabetically
        var parameters = new Dictionary<string, string>
        {
            ["amount"] = amount.ToString(),
            ["cancelUrl"] = cancelUrl,
            ["description"] = description,
            ["orderCode"] = orderCode.ToString(),
            ["returnUrl"] = returnUrl
        };

        // Sort by key alphabetically and create query string
        var sortedParams = parameters.OrderBy(x => x.Key).ToList();
        var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));

        Console.WriteLine($"Amount: {amount}");
        Console.WriteLine($"Cancel URL: {cancelUrl}");
        Console.WriteLine($"Description: {description}");
        Console.WriteLine($"Order Code: {orderCode}");
        Console.WriteLine($"Return URL: {returnUrl}");
        Console.WriteLine($"Checksum Key: {checksumKey}");
        Console.WriteLine($"Query String (sorted): {queryString}");

        // Use HMAC-SHA256 with checksum key as the secret
        var signature = GenerateHMACSHA256(queryString, checksumKey);
        Console.WriteLine($"Generated Signature: {signature}");
        Console.WriteLine($"=== End PayOS Official Signature Generation ===");

        return signature;
    }

    private string GenerateSHA256(string input)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLower();
    }

    // PayOS uses HMAC-SHA256, not just SHA256
    private string GenerateHMACSHA256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLower();
    }
}

