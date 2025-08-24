using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Commons.Constant;
using PayPal.Api;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;

namespace CusomMapOSM_Infrastructure.Services.Payment;

public class PaypalPaymentService : IPaymentService
{
    private APIContext GetAPIContext()
    {
        var config = new Dictionary<string, string>
        {
            { "mode", "sandbox" },
            { "clientId", PayPalConstant.PAYPAL_CLIENT_ID },
            { "clientSecret", PayPalConstant.PAYPAL_SECRET }
        };
        var accessToken = new OAuthTokenCredential(PayPalConstant.PAYPAL_CLIENT_ID, PayPalConstant.PAYPAL_SECRET).GetAccessToken();
        return new APIContext(accessToken) { Config = config };
    }

    private PayPal.Api.Payment CreatePayment(ProcessCreatePaymentReq req)
    {
        var apiContext = GetAPIContext();
        var payer = new Payer() { payment_method = "paypal" };
        var redirectUrls = new RedirectUrls()
        {
            cancel_url = req.CancelUrl,
            return_url = req.ReturnUrl
        };
        var details = new Details()
        {
            tax = "0",
            shipping = "0",
            subtotal = req.Total.ToString()
        };
        var amount = new Amount()
        {
            currency = "USD",
            total = req.Total.ToString(),
            details = details
        };
        var transactionList = new List<Transaction>();
        var transactionItem = new Transaction()
        {
            description = "Transaction description.",
            invoice_number = Guid.NewGuid().ToString(),
            amount = amount
        };
        transactionList.Add(transactionItem);
        var payment = new PayPal.Api.Payment()
        {
            intent = "sale",
            payer = payer,
            transactions = transactionList,
            redirect_urls = redirectUrls
        };
        return payment.Create(apiContext);
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(decimal amount, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        // Create a simple request for backward compatibility
        var simpleRequest = new ProcessPaymentReq
        {
            Total = amount,
            Purpose = "membership", // Default purpose
            PaymentGateway = PaymentGatewayEnum.PayPal
        };

        return await CreateCheckoutAsync(simpleRequest, returnUrl, cancelUrl, ct);
    }

    public async Task<Option<ApprovalUrlResponse, ErrorCustom.Error>> CreateCheckoutAsync(ProcessPaymentReq request, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        var payment = CreatePayment(new ProcessCreatePaymentReq()
        {
            Total = request.Total,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl
        });

        var approvalUrl = payment.links.FirstOrDefault(x => x.rel.ToLower() == "approval_url")?.href;
        if (approvalUrl is null)
            return Option.None<ApprovalUrlResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Paypal.ApprovalUrlNotFound", "Approval URL not found", ErrorCustom.ErrorType.Failure));

        return Option.Some<ApprovalUrlResponse, ErrorCustom.Error>(new ApprovalUrlResponse()
        {
            ApprovalUrl = approvalUrl,
            PaymentGateway = PaymentGatewayEnum.PayPal,
            SessionId = payment.id
        });
    }

    public async Task<Option<ConfirmPaymentResponse, ErrorCustom.Error>> ConfirmPaymentAsync(ConfirmPaymentReq req, CancellationToken ct)
    {
        var apiContext = GetAPIContext();

        var paymentExecution = new PaymentExecution() { payer_id = req.PayerId };
        var payment = new PayPal.Api.Payment() { id = req.PaymentId, token = req.Token };
        var executedPayment = payment.Execute(apiContext, paymentExecution);

        if (executedPayment.state.ToLower() != "approved")
            return Option.None<ConfirmPaymentResponse, ErrorCustom.Error>(new ErrorCustom.Error("Payment.Paypal.PaymentNotApproved", "Payment not approved", ErrorCustom.ErrorType.Failure));

        return Option.Some<ConfirmPaymentResponse, ErrorCustom.Error>(new ConfirmPaymentResponse()
        {
            PaymentId = req.PaymentId,
            PayerId = req.PayerId,
            Token = req.Token,
            PaymentGateway = PaymentGatewayEnum.PayPal
        });
    }

    public async Task<Option<CancelPaymentResponse, ErrorCustom.Error>> CancelPaymentAsync(CancelPaymentWithContextReq req, CancellationToken ct)
    {
        try
        {
            var apiContext = GetAPIContext();

            // For PayPal, we need to get the payment details first
            var payment = PayPal.Api.Payment.Get(apiContext, req.PaymentId);

            // Check if payment is in a cancellable state
            if (payment.state.ToLower() == "approved" || payment.state.ToLower() == "completed")
            {
                return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("Payment.Paypal.AlreadyProcessed", "Payment has already been processed and cannot be cancelled", ErrorCustom.ErrorType.Validation));
            }

            // For PayPal, cancellation is typically handled by the user not completing the payment
            // We can mark it as cancelled in our system
            return Option.Some<CancelPaymentResponse, ErrorCustom.Error>(new CancelPaymentResponse(
                "cancelled",
                PaymentGatewayEnum.PayPal.ToString()
            ));
        }
        catch (Exception ex)
        {
            return Option.None<CancelPaymentResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("Payment.Paypal.CancelFailed", $"Failed to cancel payment: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}