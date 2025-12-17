using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Services.Billing;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using CusomMapOSM_Application.Models.DTOs.Features.Transaction;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Features.Payment;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using CusomMapOSM_Application.Models.DTOs.Services;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Tests.Features.Payment;

public class SubscriptionServiceTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IMembershipPlanRepository> _mockMembershipPlanRepository;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IMembershipRepository> _mockMembershipRepository;
    private readonly Mock<IPaymentGatewayRepository> _mockPaymentGatewayRepository;
    private readonly Mock<IProrationService> _mockProrationService;
    private readonly SubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly Faker _faker;

    public SubscriptionServiceTests()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SubscriptionService>.Instance;
        _mockTransactionService = new Mock<ITransactionService>();
        _mockMembershipService = new Mock<IMembershipService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockMembershipRepository = new Mock<IMembershipRepository>();
        _mockPaymentGatewayRepository = new Mock<IPaymentGatewayRepository>();
        _mockProrationService = new Mock<IProrationService>();

        _subscriptionService = new SubscriptionService(
            _mockTransactionService.Object,
            _mockMembershipService.Object,
            _mockNotificationService.Object,
            _mockMembershipPlanRepository.Object,
            _mockOrganizationRepository.Object,
            _mockTransactionRepository.Object,
            _mockMembershipRepository.Object,
            _mockPaymentGatewayRepository.Object,
            _mockProrationService.Object,
            _logger
        );
        _faker = new Faker();
    }

    #region SubscribeToPlanAsync Tests

    [Fact]
    public async Task SubscribeToPlanAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 1;
        var request = new SubscribeRequest
        {
            UserId = userId,
            OrgId = orgId,
            PlanId = planId,
            PaymentMethod = PaymentGatewayEnum.PayOS,
            AutoRenew = false
        };

        var ownerMember = new Faker<OrganizationMember>()
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.Role, OrganizationMemberTypeEnum.Owner)
            .Generate();

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PriceMonthly, 99.99m)
            .Generate();

        var approvalResponse = new ApprovalUrlResponse
        {
            ApprovalUrl = "https://payment.url",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            SessionId = Guid.NewGuid().ToString(),
            QrCode = "qr_code",
            OrderCode = "order_123"
        };

        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(ownerMember);
        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _mockTransactionService.Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentReq>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ApprovalUrlResponse, Error>(approvalResponse));

        // Act
        var result = await _subscriptionService.SubscribeToPlanAsync(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.TransactionId.Should().Be(approvalResponse.SessionId);
        response.PaymentUrl.Should().Be(approvalResponse.ApprovalUrl);
        response.Status.Should().Be("pending");
    }

    [Fact]
    public async Task SubscribeToPlanAsync_WithNonOwnerUser_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new SubscribeRequest
        {
            UserId = userId,
            OrgId = orgId,
            PlanId = 1,
            PaymentMethod = PaymentGatewayEnum.PayOS
        };

        var member = new Faker<OrganizationMember>()
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.Role, OrganizationMemberTypeEnum.Viewer) // Not Owner
            .Generate();

        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(member);

        // Act
        var result = await _subscriptionService.SubscribeToPlanAsync(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Forbidden)
        );
    }

    [Fact]
    public async Task SubscribeToPlanAsync_WithNonExistentPlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new SubscribeRequest
        {
            UserId = userId,
            OrgId = orgId,
            PlanId = 999,
            PaymentMethod = PaymentGatewayEnum.PayOS
        };

        var ownerMember = new Faker<OrganizationMember>()
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.Role, OrganizationMemberTypeEnum.Owner)
            .Generate();

        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(ownerMember);
        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _subscriptionService.SubscribeToPlanAsync(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region UpgradePlanAsync Tests

    [Fact]
    public async Task UpgradePlanAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var currentPlanId = 1;
        var newPlanId = 2;
        var request = new UpgradeRequest
        {
            UserId = userId,
            OrgId = orgId,
            NewPlanId = newPlanId,
            PaymentMethod = PaymentGatewayEnum.PayOS,
            AutoRenew = true
        };

        var currentPlan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, currentPlanId)
            .RuleFor(p => p.PriceMonthly, 99.99m)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var newPlan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, newPlanId)
            .RuleFor(p => p.PriceMonthly, 199.99m)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, currentPlanId)
            .RuleFor(m => m.Plan, currentPlan)
            .RuleFor(m => m.BillingCycleEndDate, DateTime.UtcNow.AddDays(15))
            .Generate();

        var approvalResponse = new ApprovalUrlResponse
        {
            ApprovalUrl = "https://payment.url",
            PaymentGateway = PaymentGatewayEnum.PayOS,
            SessionId = Guid.NewGuid().ToString(),
            QrCode = "qr_code",
            OrderCode = "order_123"
        };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        // Mock proration calculation to return a positive amount due for the upgrade
        var prorationResult = new ProrationResult
        {
            AmountDue = 50m,
            UnusedCredit = 0m,
            ProratedNewPlanCost = 150m,
            DaysRemaining = 15,
            Message = "Test proration"
        };
        _mockProrationService.Setup(x => x.CalculateUpgradeProration(
            It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(prorationResult);

        _mockTransactionService.Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentReq>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<ApprovalUrlResponse, Error>(approvalResponse));

        // Act
        var result = await _subscriptionService.UpgradePlanAsync(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.TransactionId.Should().Be(approvalResponse.SessionId);
        response.ProRatedAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpgradePlanAsync_WithNoActiveMembership_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new UpgradeRequest
        {
            UserId = userId,
            OrgId = orgId,
            NewPlanId = 2,
            PaymentMethod = PaymentGatewayEnum.PayOS
        };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership, Error>(Error.NotFound("Membership.NotFound", "No active membership")));

        // Act
        var result = await _subscriptionService.UpgradePlanAsync(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region ProcessSuccessfulPaymentAsync Tests

    [Fact]
    public async Task ProcessSuccessfulPaymentAsync_WithMembershipPurpose_ShouldUpdateMembership()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 1;

        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Amount, 99.99m)
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, $"membership|{{\"UserId\":\"{userId}\",\"OrgId\":\"{orgId}\",\"PlanId\":{planId},\"AutoRenew\":false}}")
            .Generate();

        var transactionResponse = new Faker<CusomMapOSM_Domain.Entities.Transactions.Transactions>()
            .RuleFor(t => t.TransactionId, f => f.Random.Guid())
            .RuleFor(t => t.Amount, f => f.Random.Decimal(1, 1000))
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, $"membership|{{\"UserId\":\"{userId}\",\"OrgId\":\"{orgId}\",\"PlanId\":{planId},\"AutoRenew\":false}}")
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, planId)
            .Generate();

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CusomMapOSM_Domain.Entities.Transactions.Transactions, Error>(transaction));
        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(userId, orgId, planId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockNotificationService.Setup(x => x.CreateTransactionCompletedNotificationAsync(userId, transaction.Amount, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _subscriptionService.ProcessSuccessfulPaymentAsync(transactionId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.MembershipUpdated.Should().BeTrue();
        response.NotificationSent.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessSuccessfulPaymentAsync_WithUpgradePurpose_ShouldUpgradeMembership()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 2;

        var transaction = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, transactionId)
            .RuleFor(t => t.Amount, 199.99m)
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, $"upgrade|{{\"UserId\":\"{userId}\",\"OrgId\":\"{orgId}\",\"PlanId\":{planId},\"AutoRenew\":true}}")
            .Generate();

        var transactionResponse = new Faker<CusomMapOSM_Domain.Entities.Transactions.Transactions>()
            .RuleFor(t => t.TransactionId, f => f.Random.Guid())
            .RuleFor(t => t.Amount, f => f.Random.Decimal(1, 1000))
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, $"upgrade|{{\"UserId\":\"{userId}\",\"OrgId\":\"{orgId}\",\"PlanId\":{planId},\"AutoRenew\":true}}")
            .RuleFor(t => t.PaymentGatewayId, f => f.Random.Guid())
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, planId)
            .Generate();

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<CusomMapOSM_Domain.Entities.Transactions.Transactions, Error>(transaction));
        _mockTransactionRepository.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(userId, orgId, planId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockNotificationService.Setup(x => x.CreateTransactionCompletedNotificationAsync(userId, transaction.Amount, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _subscriptionService.ProcessSuccessfulPaymentAsync(transactionId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.MembershipUpdated.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessSuccessfulPaymentAsync_WithNonExistentTransaction_ShouldReturnError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mockTransactionService.Setup(x => x.GetTransactionAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<CusomMapOSM_Domain.Entities.Transactions.Transactions, Error>(Error.NotFound("Transaction.NotFound", "Transaction not found")));

        // Act
        var result = await _subscriptionService.ProcessSuccessfulPaymentAsync(transactionId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetPaymentHistoryAsync Tests

    [Fact]
    public async Task GetPaymentHistoryAsync_WithValidUserId_ShouldReturnHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactions = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, Guid.NewGuid())
            .RuleFor(t => t.MembershipId, f => f.Random.Guid())
            .RuleFor(t => t.Amount, 99.99m)
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, "membership")
            .RuleFor(t => t.TransactionDate, DateTime.UtcNow)
            .RuleFor(t => t.CreatedAt, DateTime.UtcNow)
            .RuleFor(t => t.PaymentGatewayId, Guid.NewGuid())
            .Generate(5);

        var paymentGateway = new Faker<PaymentGateway>()
            .RuleFor(pg => pg.GatewayId, f => f.Random.Guid())
            .RuleFor(pg => pg.Name, "PayOS")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);
        _mockPaymentGatewayRepository.Setup(x => x.GetByGatewayIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentGateway);

        // Act
        var result = await _subscriptionService.GetPaymentHistoryAsync(userId, 1, 20);

        // Assert
        result.HasValue.Should().BeTrue();
        var history = result.ValueOrFailure();
        history.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_WithNoTransactions_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockTransactionRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transactions>());

        // Act
        var result = await _subscriptionService.GetPaymentHistoryAsync(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        var history = result.ValueOrFailure();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactions = new Faker<Transactions>()
            .RuleFor(t => t.TransactionId, Guid.NewGuid())
            .RuleFor(t => t.MembershipId, f => f.Random.Guid())
            .RuleFor(t => t.Amount, 99.99m)
            .RuleFor(t => t.Status, "completed")
            .RuleFor(t => t.Purpose, "membership")
            .RuleFor(t => t.PaymentGatewayId, Guid.NewGuid())
            .Generate(25);

        var paymentGateway = new Faker<PaymentGateway>()
            .RuleFor(pg => pg.GatewayId, f => f.Random.Guid())
            .RuleFor(pg => pg.Name, "PayOS")
            .Generate();

        _mockTransactionRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);
        _mockPaymentGatewayRepository.Setup(x => x.GetByGatewayIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentGateway);

        // Act
        var result = await _subscriptionService.GetPaymentHistoryAsync(userId, 1, 10);

        // Assert
        result.HasValue.Should().BeTrue();
        var history = result.ValueOrFailure();
        history.Should().HaveCount(10); // First page should have 10 items
    }

    #endregion
}

