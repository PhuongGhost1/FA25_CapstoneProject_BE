using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_API.Endpoints.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.Transaction;

public class TransactionEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Faker _faker;

    public TransactionEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => { services.AddScoped(_ => _mockTransactionService.Object); });
        });
    }

    [Fact]
    public async Task ProcessPayment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayOS,
            Total = 99.99m,
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = null,
            PlanId = 1,
            AutoRenew = true
        };

        var approvalResponse = new ApprovalUrlResponse
        {
            ApprovalUrl = "https://payos.vn/checkout",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            SessionId = "session_123"
        };

        _mockTransactionService.Setup(x => x.ProcessPaymentAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ApprovalUrlResponse, Error>(approvalResponse));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/process-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApprovalUrlResponse>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(approvalResponse);
    }

    [Fact]
    public async Task ProcessPayment_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayOS,
            Total = 99.99m,
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = null,
            PlanId = 1,
            AutoRenew = true
        };

        var error = new Error("Payment.Gateway.NotFound", "Payment gateway not found", ErrorType.NotFound);

        _mockTransactionService.Setup(x => x.ProcessPaymentAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<ApprovalUrlResponse, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/process-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmPayment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ConfirmPaymentWithContextReq
        {
            TransactionId = Guid.NewGuid(),
            PaymentGateway = PaymentGatewayEnum.PayOS,
            PaymentId = "payment_123",
            OrderCode = "order_123",
            Signature = "signature_123",
            Purpose = "membership"
        };

        var confirmResponse = new { Success = true, TransactionId = request.TransactionId };

        _mockTransactionService.Setup(x => x.ConfirmPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<object, Error>(confirmResponse));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/confirm-payment-with-context", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConfirmPayment_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ConfirmPaymentWithContextReq
        {
            TransactionId = Guid.NewGuid(),
            PaymentGateway = PaymentGatewayEnum.PayOS,
            PaymentId = "payment_123",
            OrderCode = "order_123",
            Signature = "signature_123",
            Purpose = "membership"
        };

        var error = new Error("Payment.Confirmation.Failed", "Payment confirmation failed", ErrorType.Failure);

        _mockTransactionService.Setup(x => x.ConfirmPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<object, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/confirm-payment-with-context", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelPayment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CancelPaymentWithContextReq(
            PaymentGatewayEnum.PayOS,
            "payment_123",
            "", // PayerId (not used for PayOS)
            "", // Token (not used for PayOS)
            "", // PaymentIntentId (not used for PayOS)
            "", // ClientSecret (not used for PayOS)
            "ORDER_123", //SectionId
            "signature_123", // OrderCode
            "", // Signature  
            Guid.NewGuid() // TransactionId
        );

        var cancelResponse = new CancelPaymentResponse("cancelled", PaymentGatewayEnum.PayOS.ToString());

        _mockTransactionService.Setup(x => x.CancelPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CancelPaymentResponse, Error>(cancelResponse));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/cancel-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelPayment_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CancelPaymentWithContextReq(
            PaymentGatewayEnum.PayOS,
            "payment_123",
            "", // PayerId (not used for PayOS)
            "", // Token (not used for PayOS)
            "", // PaymentIntentId (not used for PayOS)
            "", // ClientSecret (not used for PayOS)
            "ORDER_123", //SectionId
            "signature_123", // OrderCode
            "", // Signature  
            Guid.NewGuid() // TransactionId
        );

        var error = new Error("Payment.Cancellation.Failed", "Payment cancellation failed", ErrorType.Failure);

        _mockTransactionService.Setup(x => x.CancelPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<CancelPaymentResponse, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/cancel-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransaction_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var transactionId = Guid.NewGuid();
        var transaction = new Transactions
        {
            TransactionId = transactionId,
            Amount = 99.99m,
            Status = "completed",
            PaymentGatewayId = Guid.NewGuid(),
            Purpose = "membership"
        };

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<Transactions, Error>(transaction));

        // Act
        var response = await client.GetAsync($"/transaction/{transactionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Transactions>();
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public async Task GetTransaction_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var transactionId = Guid.NewGuid();

        var error = new Error("Transaction.NotFound", "Transaction not found", ErrorType.NotFound);

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<Transactions, Error>(error));

        // Act
        var response = await client.GetAsync($"/transaction/{transactionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProcessPayment_WithInvalidAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayOS,
            Total = 0, // Invalid amount
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = null,
            PlanId = 1,
            AutoRenew = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/transaction/process-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}