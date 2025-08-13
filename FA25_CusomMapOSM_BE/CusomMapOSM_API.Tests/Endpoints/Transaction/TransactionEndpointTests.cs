using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_API.Endpoints.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
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
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockTransactionService.Object);
            });
        });
    }

    [Fact]
    public async Task ProcessPayment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayPal,
            Total = 99.99m,
            Purpose = "membership",
            MembershipId = Guid.NewGuid()
        };

        var approvalResponse = new ApprovalUrlResponse("session_123", "https://paypal.com/checkout");

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
            PaymentGateway = PaymentGatewayEnum.PayPal,
            Total = 99.99m,
            Purpose = "membership",
            MembershipId = Guid.NewGuid()
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
            PaymentGateway = PaymentGatewayEnum.PayPal,
            PaymentId = "payment_123",
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            PlanId = 1
        };

        var confirmResponse = new
        {
            MembershipId = Guid.NewGuid(),
            TransactionId = request.TransactionId,
            AccessToolsGranted = true
        };

        _mockTransactionService.Setup(x => x.ConfirmPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<object, Error>(confirmResponse));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/confirm-payment-with-context", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransaction_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var transactionId = Guid.NewGuid();
        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Amount, 99.99m)
            .RuleFor(t => t.Status, "success")
            .Generate();

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<Transactions, Error>(transaction));

        // Act
        var response = await client.GetAsync($"/transaction/get-transaction/{transactionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Transactions>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(transaction);
    }

    [Fact]
    public async Task CancelPayment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CancelPaymentWithContextReq
        {
            TransactionId = Guid.NewGuid(),
            PaymentGateway = PaymentGatewayEnum.PayPal
        };

        var cancelResponse = new CancelPaymentResponse("cancelled", PaymentGatewayEnum.PayPal.ToString());

        _mockTransactionService.Setup(x => x.CancelPaymentWithContextAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CancelPaymentResponse, Error>(cancelResponse));

        // Act
        var response = await client.PostAsJsonAsync("/transaction/cancel-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CancelPaymentResponse>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cancelResponse);
    }

    [Fact]
    public async Task ProcessPayment_WithNullRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<ProcessPaymentReq>("/transaction/process-payment", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransaction_WithInvalidRoute_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/transaction/invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
