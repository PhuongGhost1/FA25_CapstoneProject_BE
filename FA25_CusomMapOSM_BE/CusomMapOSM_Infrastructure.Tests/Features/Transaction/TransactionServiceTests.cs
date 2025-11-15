using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Services.Payment;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Optional;
using Xunit;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;

namespace CusomMapOSM_Infrastructure.Tests.Features.Transaction;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IPaymentGatewayRepository> _mockPaymentGatewayRepository;
    private readonly Mock<HangfireEmailService> _mockHangfireEmailService;
    private readonly Mock<IEmailNotificationService> _mockEmailNotificationService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMembershipRepository> _mockMembershipRepository;
    private readonly Mock<IMembershipPlanRepository> _mockMembershipPlanRepository;
    private readonly Mock<ILogger<TransactionService>> _mockLogger;
    private readonly TransactionService _transactionService;
    private readonly Faker _faker;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockMembershipService = new Mock<IMembershipService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockPaymentGatewayRepository = new Mock<IPaymentGatewayRepository>();
        _mockHangfireEmailService = new Mock<HangfireEmailService>();
        _mockEmailNotificationService = new Mock<IEmailNotificationService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMembershipRepository = new Mock<IMembershipRepository>();
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();
        _mockLogger = new Mock<ILogger<TransactionService>>();

        _transactionService = new TransactionService(
            _mockTransactionRepository.Object,
            _mockPaymentService.Object,
            _mockMembershipService.Object,
            _mockServiceProvider.Object,
            _mockPaymentGatewayRepository.Object,
            _mockHangfireEmailService.Object,
            _mockEmailNotificationService.Object,
            _mockUserRepository.Object,
            _mockMembershipRepository.Object,
            _mockMembershipPlanRepository.Object,
            _mockLogger.Object
        );


        _faker = new Faker();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidRequest_ShouldReturnApprovalUrl()
    {
        // Arrange
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayOS,
            Total = 99.99m,
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            PlanId = 1,
            AutoRenew = true
        };

        var gatewayId = Guid.NewGuid();
        var pendingTransaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, Guid.NewGuid())
            .RuleFor(t => t.PaymentGatewayId, gatewayId)
            .RuleFor(t => t.Amount, request.Total)
            .RuleFor(t => t.Status, "pending")
            .Generate();

        var approvalResponse = new ApprovalUrlResponse
        {
            ApprovalUrl = "https://payos.vn/checkout",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            SessionId = "session_123",
            QrCode = "qr_code_data",
            OrderCode = "ORDER_123"
        };

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
        result.ValueOr(default(ApprovalUrlResponse)).Should().BeEquivalentTo(approvalResponse);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidGateway_ShouldReturnError()
    {
        // Arrange
        var request = new ProcessPaymentReq
        {
            PaymentGateway = PaymentGatewayEnum.PayOS,
            Total = 99.99m,
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            PlanId = 1,
            AutoRenew = true
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
            PaymentGateway = PaymentGatewayEnum.PayOS,
            PaymentId = "payment_123",
            Purpose = "membership",
            UserId = Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            PlanId = 1,
            OrderCode = "ORDER_123",
            Signature = "signature_123"
        };

        var existingTransaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Status, "pending")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var confirmPaymentResponse = new ConfirmPaymentResponse
        {
            PaymentId = "payment_123",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            OrderCode = "ORDER_123",
            Signature = "signature_123"
        };

        _mockPaymentService.Setup(x =>
                x.ConfirmPaymentAsync(It.IsAny<ConfirmPaymentReq>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ConfirmPaymentResponse, Error>(confirmPaymentResponse));

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
        result.ValueOr(default(Transactions)).Should().BeEquivalentTo(transaction);
    }

    [Fact]
    public async Task CancelPaymentWithContextAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
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

        var gatewayId = Guid.NewGuid();

        _mockPaymentGatewayRepository.Setup(x => x.GetByIdAsync(request.PaymentGateway, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGateway { GatewayId = gatewayId });

        var cancelResponse = new CancelPaymentResponse("cancelled", PaymentGatewayEnum.PayOS.ToString());

        _mockPaymentService.Setup(x => x.CancelPaymentAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CancelPaymentResponse, Error>(cancelResponse));

        // Act
        var result = await _transactionService.CancelPaymentWithContextAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOr(default(CancelPaymentResponse));
        response.Status.Should().Be("cancelled");
    }

    [Fact]
    public void GetPaymentService_WithPayOSGateway_ShouldReturnPaymentService()
    {
        // Act
        var result = _transactionService.GetPaymentService(PaymentGatewayEnum.PayOS);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_mockPaymentService.Object);
    }

    [Fact]
    public void GetPaymentService_WithInvalidGateway_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidGateway = PaymentGatewayEnum.PayPal; // Using PayPal as invalid since we only support PayOS

        // Act & Assert
        var action = () => _transactionService.GetPaymentService(invalidGateway);
        action.Should().Throw<ArgumentException>().WithMessage("Invalid payment gateway");
    }
}