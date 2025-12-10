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
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Optional;
using Xunit;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using Hangfire;
using Hangfire.MemoryStorage;

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

    static TransactionServiceTests()
    {
        // Initialize Hangfire with in-memory storage for tests (only once, before any test runs)
        var storage = new MemoryStorage();
        GlobalConfiguration.Configuration.UseStorage(storage);
        JobStorage.Current = storage;
    }

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

        // GetPaymentGatewayId now uses internal constants, not repository
        // The service uses GetPaymentGatewayId which returns a constant GUID

        _mockTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransaction);

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(pendingTransaction.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransaction);

        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransaction);

        // Setup service provider to return payment service
        var paymentServices = new List<IPaymentService> { _mockPaymentService.Object };
        _mockServiceProvider.Setup(x => x.GetServices<IPaymentService>())
            .Returns(paymentServices);

        _mockPaymentService.Setup(x => x.CreateCheckoutAsync(
                request,
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
    public async Task ProcessPaymentAsync_WithTransactionCreationFailure_ShouldReturnError()
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

        _mockTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _transactionService.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Code.Should().Be("Transaction.Create.Failed")
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
            .RuleFor(t => t.Purpose, "membership|{\"UserId\":\"" + request.UserId + "\",\"OrgId\":\"" + request.OrgId + "\",\"PlanId\":" + request.PlanId + ",\"AutoRenew\":true}")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var confirmPaymentResponse = new ConfirmPaymentResponse
        {
            PaymentId = "payment_123",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            OrderCode = "ORDER_123",
            Signature = "signature_123"
        };

        // Setup service provider to return payment service
        var paymentServices = new List<IPaymentService> { _mockPaymentService.Object };
        _mockServiceProvider.Setup(x => x.GetServices<IPaymentService>())
            .Returns(paymentServices);

        _mockPaymentService.Setup(x =>
                x.ConfirmPaymentAsync(It.IsAny<ConfirmPaymentReq>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ConfirmPaymentResponse, Error>(confirmPaymentResponse));

        var plan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, f => f.Random.Int())
            .RuleFor(p => p.PlanName, "Test Plan")
            .Generate();

        var membership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.EndDate, DateTime.UtcNow.AddMonths(1))
            .RuleFor(m => m.Plan, plan)
            .Generate();

        var user = new Faker<CusomMapOSM_Domain.Entities.Users.User>()
            .RuleFor(u => u.UserId, request.UserId!.Value)
            .RuleFor(u => u.Email, "test@example.com")
            .RuleFor(u => u.FullName, "Test User")
            .Generate();

        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(
                request.UserId!.Value,
                request.OrgId!.Value,
                request.PlanId!.Value,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(request.PlanId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockUserRepository.Setup(x => x.GetUserByIdAsync(request.UserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailNotificationService.Setup(x => x.SendTransactionCompletedNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);


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

        // Setup service provider to return payment service
        var paymentServices = new List<IPaymentService> { _mockPaymentService.Object };
        _mockServiceProvider.Setup(x => x.GetServices<IPaymentService>())
            .Returns(paymentServices);

        var cancelResponse = new CancelPaymentResponse("cancelled", PaymentGatewayEnum.PayOS.ToString());

        _mockPaymentService.Setup(x => x.CancelPaymentAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CancelPaymentResponse, Error>(cancelResponse));

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(request.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Transactions { TransactionId = request.TransactionId, PaymentGatewayId = Guid.NewGuid(), Amount = 99.99m, Purpose = "membership" });

        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Transactions { TransactionId = request.TransactionId, PaymentGatewayId = Guid.NewGuid(), Amount = 99.99m, Purpose = "membership", Status = "cancelled" });

        // Act
        var result = await _transactionService.CancelPaymentWithContextAsync(request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOr(default(CancelPaymentResponse));
        response.Status.Should().Be("cancelled");
    }

    [Fact]
    public void GetPaymentService_WithUnsupportedGateway_ShouldThrowArgumentException()
    {
        // Arrange
        var paymentServices = new List<IPaymentService>();
        _mockServiceProvider.Setup(x => x.GetServices<IPaymentService>())
            .Returns(paymentServices);

        // Act & Assert
        var action = () => _transactionService.GetPaymentService(PaymentGatewayEnum.PayPal);
        action.Should().Throw<ArgumentException>();
    }

    #region CreateTransactionRecordAsync Tests

    [Fact]
    public async Task CreateTransactionRecordAsync_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var amount = 99.99m;
        var purpose = "membership";
        var membershipId = Guid.NewGuid();
        var status = "pending";

        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, Guid.NewGuid())
            .RuleFor(t => t.PaymentGatewayId, gatewayId)
            .RuleFor(t => t.Amount, amount)
            .RuleFor(t => t.Purpose, purpose)
            .RuleFor(t => t.MembershipId, membershipId)
            .RuleFor(t => t.Status, status)
            .Generate();

        _mockTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.CreateTransactionRecordAsync(gatewayId, amount, purpose, membershipId, null, status, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Transactions)).Amount.Should().Be(amount);
        result.ValueOr(default(Transactions)).Purpose.Should().Be(purpose);
        _mockTransactionRepository.Verify(x => x.CreateAsync(It.Is<Transactions>(t =>
            t.PaymentGatewayId == gatewayId &&
            t.Amount == amount &&
            t.Purpose == purpose), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateTransactionStatusAsync Tests

    [Fact]
    public async Task UpdateTransactionStatusAsync_WithValidTransaction_ShouldUpdateStatus()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var newStatus = "success";

        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Status, "pending")
            .Generate();

        var updatedTransaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Status, newStatus)
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTransaction);

        // Act
        var result = await _transactionService.UpdateTransactionStatusAsync(transactionId, newStatus, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Transactions)).Status.Should().Be(newStatus);
        transaction.Status.Should().Be(newStatus);
    }

    [Fact]
    public async Task UpdateTransactionStatusAsync_WithNonExistentTransaction_ShouldReturnError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var newStatus = "success";

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transactions?)null);

        // Act
        var result = await _transactionService.UpdateTransactionStatusAsync(transactionId, newStatus, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region UpdateTransactionGatewayInfoAsync Tests

    [Fact]
    public async Task UpdateTransactionGatewayInfoAsync_WithValidTransaction_ShouldUpdateGatewayReference()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var gatewayReference = "session_12345";

        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.TransactionReference, "old_reference")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transactions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.UpdateTransactionGatewayInfoAsync(transactionId, gatewayReference, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        transaction.TransactionReference.Should().Be(gatewayReference);
        _mockTransactionRepository.Verify(x => x.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTransactionGatewayInfoAsync_WithNonExistentTransaction_ShouldReturnError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var gatewayReference = "session_12345";

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transactions?)null);

        // Act
        var result = await _transactionService.UpdateTransactionGatewayInfoAsync(transactionId, gatewayReference, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetPaymentGatewayId Tests

    [Fact]
    public void GetPaymentGatewayId_WithPayOS_ShouldReturnGuid()
    {
        // Act
        var result = _transactionService.GetPaymentGatewayId(PaymentGatewayEnum.PayOS);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Guid)).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GetPaymentGatewayId_WithVNPay_ShouldReturnGuid()
    {
        // Act
        var result = _transactionService.GetPaymentGatewayId(PaymentGatewayEnum.VNPay);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Guid)).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GetPaymentGatewayId_WithStripe_ShouldReturnGuid()
    {
        // Act
        var result = _transactionService.GetPaymentGatewayId(PaymentGatewayEnum.Stripe);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Guid)).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GetPaymentGatewayId_WithPayPal_ShouldReturnGuid()
    {
        // Act
        var result = _transactionService.GetPaymentGatewayId(PaymentGatewayEnum.PayPal);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Guid)).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GetPaymentGatewayId_WithBankTransfer_ShouldReturnGuid()
    {
        // Act
        var result = _transactionService.GetPaymentGatewayId(PaymentGatewayEnum.BankTransfer);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOr(default(Guid)).Should().NotBe(Guid.Empty);
    }

    #endregion

    #region GetTransactionAsync Tests

    [Fact]
    public async Task GetTransactionAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transactions?)null);

        // Act
        var result = await _transactionService.GetTransactionAsync(transactionId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion
}