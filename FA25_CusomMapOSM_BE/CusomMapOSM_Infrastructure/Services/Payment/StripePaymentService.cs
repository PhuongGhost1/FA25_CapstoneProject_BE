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
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(amount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Purchase"
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = returnUrl,
            CancelUrl = cancelUrl
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, new RequestOptions { ApiKey = StripeConstant.STRIPE_SECRET_KEY });

        return Option.Some<ApprovalUrlResponse, Error>(new ApprovalUrlResponse
        {
            ApprovalUrl = session.Url,
            PaymentGateway = PaymentGatewayEnum.Stripe,
            SessionId = session.Id
        });
    }

    public async Task<Option<ConfirmPaymentResponse, Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(req.PaymentIntentId, new PaymentIntentGetOptions { ClientSecret = StripeConstant.STRIPE_SECRET_KEY });

        if (paymentIntent.Status == "succeeded")
        {
            return Option.Some<ConfirmPaymentResponse, Error>(new ConfirmPaymentResponse
            {
                PaymentId = req.PaymentIntentId,
                PaymentGateway = PaymentGatewayEnum.Stripe,
                PaymentIntentId = req.PaymentIntentId,
                ClientSecret = req.ClientSecret
            });
        }

        return Option.None<ConfirmPaymentResponse, Error>(new Error("Payment.Stripe.Failed", "Payment not successful", ErrorType.Validation));
    }

    public async Task<Option<CancelPaymentResponse, Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(req.PaymentIntentId, new PaymentIntentGetOptions { ClientSecret = StripeConstant.STRIPE_SECRET_KEY });

            // Check if payment intent is in a cancellable state
            if (paymentIntent.Status == "succeeded" || paymentIntent.Status == "processing")
            {
                return Option.None<CancelPaymentResponse, Error>(
                    new Error("Payment.Stripe.AlreadyProcessed", "Payment has already been processed and cannot be cancelled", ErrorType.Validation));
            }

            // For Stripe, we can cancel the payment intent if it's still in a pending state
            if (paymentIntent.Status == "requires_payment_method" || paymentIntent.Status == "requires_confirmation")
            {
                var cancelOptions = new PaymentIntentCancelOptions();
                await service.CancelAsync(req.PaymentIntentId, cancelOptions, new RequestOptions { ApiKey = StripeConstant.STRIPE_SECRET_KEY });
            }

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
