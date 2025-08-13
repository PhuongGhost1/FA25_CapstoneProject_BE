using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Infrastructure.Services.Payment;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using Xunit;

namespace CusomMapOSM_Infrastructure.Tests.Features.Transaction;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<IUserAccessToolService> _mockUserAccessToolService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IPaymentGatewayRepository> _mockPaymentGatewayRepository;
    private readonly TransactionService _transactionService;
    private readonly Faker _faker;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockMembershipService = new Mock<IMembershipService>();
        _mockUserAccessToolService = new Mock<IUserAccessToolService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockPaymentGatewayRepository = new Mock<IPaymentGatewayRepository>();

        _transactionService = new TransactionService(
            _mockTransactionRepository.Object,
            _mockPaymentService.Object,
            _mockMembershipService.Object,
            _mockUserAccessToolService.Object,
            _mockServiceProvider.Object,
            _mockPaymentGatewayRepository.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidRequest_ShouldReturnApprovalUrl()
    {
        // Arrange
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayPal,
            Total = 99.99m,
            Purpose = "membership",
            MembershipId = Guid.NewGuid()
        };

        var gatewayId = Guid.NewGuid();
        var pendingTransaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, Guid.NewGuid())
            .RuleFor(t => t.PaymentGatewayId, gatewayId)
            .RuleFor(t => t.Amount, request.Total)
            .RuleFor(t => t.Status, "pending")
            .Generate();

        var approvalResponse = new ApprovalUrlResponse("session_123", "https://paypal.com/checkout");

        _mockPaymentGatewayRepository.Setup(x => x.GetByIdAsync(request.PaymentGateway, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGateway { GatewayId = gatewayId });

        _mockTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransaction);

        _mockPaymentService.Setup(x => x.CreateCheckoutAsync(
                request.Total,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ApprovalUrlResponse, Error>(approvalResponse));

        // Act
        var result = await _transactionService.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeEquivalentTo(approvalResponse);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidGateway_ShouldReturnError()
    {
        // Arrange
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayPal,
            Total = 99.99m,
            Purpose = "membership",
            MembershipId = Guid.NewGuid()
        };

        _mockPaymentGatewayRepository.Setup(x => x.GetByIdAsync(request.PaymentGateway, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentGateway?)null);

        // Act
        var result = await _transactionService.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Code.Should().Be("Payment.Gateway.NotFound")
        );
    }

    [Fact]
    public async Task ConfirmPaymentWithContextAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new ConfirmPaymentWithContextReq
        {
            TransactionId = transactionId,
            PaymentGateway = PaymentGatewayEnum.PayPal,
            PaymentId = "payment_123",
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            PlanId = 1
        };

        var existingTransaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Status, "pending")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPaymentService.Setup(x => x.ConfirmPaymentAsync(It.IsAny<ConfirmPaymentReq>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<object, Error>(new { status = "confirmed" }));

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.EndDate, DateTime.UtcNow.AddMonths(1))
            .Generate();

        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(
                request.UserId!.Value,
                request.OrgId!.Value,
                request.PlanId!.Value,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));

        // Act
        var result = await _transactionService.ConfirmPaymentWithContextAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task GetTransactionAsync_WithValidId_ShouldReturnTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Amount, 99.99m)
            .RuleFor(t => t.Status, "success")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.GetTransactionAsync(transactionId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeEquivalentTo(transaction);
    }

    [Fact]
    public async Task CancelPaymentWithContextAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CancelPaymentWithContextReq
        {
            TransactionId = Guid.NewGuid(),
            PaymentGateway = PaymentGatewayEnum.PayPal
        };

        var gatewayId = Guid.NewGuid();

        _mockPaymentGatewayRepository.Setup(x => x.GetByIdAsync(request.PaymentGateway, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGateway { GatewayId = gatewayId });

        // Act
        var result = await _transactionService.CancelPaymentWithContextAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Status.Should().Be("cancelled");
    }

    [Theory]
    [InlineData(PaymentGatewayEnum.PayPal)]
    [InlineData(PaymentGatewayEnum.Stripe)]
    [InlineData(PaymentGatewayEnum.PayOS)]
    public void GetPaymentService_WithValidGateway_ShouldReturnCorrectService(PaymentGatewayEnum gateway)
    {
        // Arrange
        var mockPaypalService = new Mock<PaypalPaymentService>();
        var mockStripeService = new Mock<StripePaymentService>();
        var mockPayOSService = new Mock<PayOSPaymentService>();

        _mockServiceProvider.Setup(x => x.GetRequiredService<PaypalPaymentService>())
            .Returns(mockPaypalService.Object);
        _mockServiceProvider.Setup(x => x.GetRequiredService<StripePaymentService>())
            .Returns(mockStripeService.Object);
        _mockServiceProvider.Setup(x => x.GetRequiredService<PayOSPaymentService>())
            .Returns(mockPayOSService.Object);

        // Act
        var result = _transactionService.GetPaymentService(gateway);

        // Assert
        result.Should().NotBeNull();
    }
}
