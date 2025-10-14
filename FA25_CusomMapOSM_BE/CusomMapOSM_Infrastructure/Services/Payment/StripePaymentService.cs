using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using Optional;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using Stripe.Checkout;
using Stripe;
using CusomMapOSM_Commons.Constant;

namespace CusomMapOSM_Infrastructure.Services.Payment;

public class StripePaymentService : IPaymentService
{
    public async Task<Option<ApprovalUrlResponse, Error>> CreateCheckoutAsync(decimal amount, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        // Create a simple request for backward compatibility
        var simpleRequest = new ProcessPaymentReq
        {
            Total = amount,
            Purpose = "membership", // Default purpose
            PaymentGateway = PaymentGatewayEnum.Stripe
        };

        return await CreateCheckoutAsync(simpleRequest, returnUrl, cancelUrl, ct);
    }

    public async Task<Option<ApprovalUrlResponse, Error>> CreateCheckoutAsync(ProcessPaymentReq request, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = returnUrl,
            CancelUrl = cancelUrl
        };

        // Determine line items based on purpose
        if (request.Purpose?.ToLower() == "membership")
        {
            // Membership purchase (default)
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(request.Total * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "CustomMapOSM Membership"
                    }
                },
                Quantity = 1
            });
        }

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(options, new RequestOptions { ApiKey = StripeConstant.STRIPE_SECRET_KEY });

            Console.WriteLine($"=== Stripe Session Created ===");
            Console.WriteLine($"Session ID: {session.Id}");
            Console.WriteLine($"Session URL: {session.Url}");
            Console.WriteLine($"Payment Status: {session.PaymentStatus}");
            Console.WriteLine($"=== End Stripe Session Created ===");

            return Option.Some<ApprovalUrlResponse, Error>(new ApprovalUrlResponse
            {
                ApprovalUrl = session.Url ?? "",
                PaymentGateway = PaymentGatewayEnum.Stripe,
                SessionId = session.Id
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== Stripe Session Creation Error ===");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"=== End Stripe Session Creation Error ===");

            return Option.None<ApprovalUrlResponse, Error>(new Error("Payment.Stripe.CreateFailed", $"Failed to create Stripe session: {ex.Message}", ErrorType.Failure));
        }
    }

    public async Task<Option<ConfirmPaymentResponse, Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct)
    {
        try
        {
            // For Stripe Checkout, we can use SessionId or PaymentId (which contains the session ID)
            var sessionId = req.SessionId ?? req.PaymentId;

            if (string.IsNullOrEmpty(sessionId))
            {
                return Option.None<ConfirmPaymentResponse, Error>(new Error("Payment.Stripe.MissingSessionId", "Session ID or Payment ID is required for Stripe Checkout", ErrorType.Validation));
            }

            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(
                sessionId,
                new SessionGetOptions(),
                new RequestOptions { ApiKey = StripeConstant.STRIPE_SECRET_KEY }
            );

            // Check if payment was successful
            if (session.PaymentStatus == "paid")
            {
                return Option.Some<ConfirmPaymentResponse, Error>(new ConfirmPaymentResponse
                {
                    PaymentId = session.PaymentIntentId ?? session.Id,
                    PaymentGateway = PaymentGatewayEnum.Stripe,
                    SessionId = session.Id,
                    OrderCode = session.Id
                });
            }

            return Option.None<ConfirmPaymentResponse, Error>(new Error("Payment.Stripe.NotPaid", $"Payment not successful. Status: {session.PaymentStatus}", ErrorType.Validation));
        }
        catch (Exception ex)
        {
            return Option.None<ConfirmPaymentResponse, Error>(new Error("Payment.Stripe.Exception", $"Exception occurred: {ex.Message}", ErrorType.Failure));
        }
    }

    public async Task<Option<CancelPaymentResponse, Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        try
        {
            // For Stripe Checkout, we can use PaymentId as SessionId since they're the same for Stripe
            var sessionId = req.PaymentId; // PaymentId contains the session ID for Stripe

            if (string.IsNullOrEmpty(sessionId))
            {
                return Option.None<CancelPaymentResponse, Error>(new Error("Payment.Stripe.MissingSessionId", "Session ID is required for Stripe Checkout", ErrorType.Validation));
            }

            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId, new SessionGetOptions(), new RequestOptions { ApiKey = StripeConstant.STRIPE_SECRET_KEY });

            // Check if payment is already completed
            if (session.PaymentStatus == "paid")
            {
                return Option.None<CancelPaymentResponse, Error>(
                    new Error("Payment.Stripe.AlreadyPaid", "Payment has already been completed and cannot be cancelled", ErrorType.Validation));
            }

            // For Stripe Checkout, cancellation is typically handled by the user not completing the payment
            // The session will expire automatically after a certain time
            return Option.Some<CancelPaymentResponse, Error>(new CancelPaymentResponse(
                "cancelled",
                PaymentGatewayEnum.Stripe.ToString()
            ));
        }
        catch (Exception ex)
        {
            return Option.None<CancelPaymentResponse, Error>(
                new Error("Payment.Stripe.CancelFailed", $"Failed to cancel payment: {ex.Message}", ErrorType.Failure));
        }
    }
}
