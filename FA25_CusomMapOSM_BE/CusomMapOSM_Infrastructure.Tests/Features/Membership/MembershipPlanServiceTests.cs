using Bogus;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Features.Membership;
using FluentAssertions;
using Moq;
using Xunit;

namespace CusomMapOSM_Infrastructure.Tests.Features.Membership;

public class MembershipPlanServiceTests
{
    private readonly Mock<IMembershipPlanRepository> _mockMembershipPlanRepository;
    private readonly MembershipPlanService _membershipPlanService;
    private readonly Faker _faker;

    public MembershipPlanServiceTests()
    {
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();
        _membershipPlanService = new MembershipPlanService(_mockMembershipPlanRepository.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task GetActivePlansAsync_WithActivePlans_ShouldReturnPlans()
    {
        // Arrange
        var activePlans = new Faker<Plan>()
            .RuleFor(p => p.PlanId, f => f.Random.Int(1, 10))
            .RuleFor(p => p.PlanName, f => f.PickRandom("Basic", "Pro", "Enterprise"))
            .RuleFor(p => p.PriceMonthly, f => f.Random.Decimal(9.99m, 99.99m))
            .RuleFor(p => p.MaxMapsPerMonth, f => f.Random.Int(25, 1000))
            .RuleFor(p => p.ExportQuota, f => f.Random.Int(50, 500))
            .RuleFor(p => p.MaxUsersPerOrg, f => f.Random.Int(5, 100))
            .RuleFor(p => p.IsActive, true)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent())
            .Generate(3);

        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var result = await _membershipPlanService.GetActivePlansAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(activePlans);

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithNoActivePlans_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyPlans = new List<Plan>();

        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPlans);

        // Act
        var result = await _membershipPlanService.GetActivePlansAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _membershipPlanService.GetActivePlansAsync(CancellationToken.None));

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _membershipPlanService.GetActivePlansAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithValidId_ShouldReturnPlan()
    {
        // Arrange
        var planId = 3;
        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 100)
            .RuleFor(p => p.ExportQuota, 200)
            .RuleFor(p => p.MaxUsersPerOrg, 20)
            .RuleFor(p => p.IsActive, true)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent())
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(plan);
        result!.PlanId.Should().Be(planId);
        result.PlanName.Should().Be("Pro");

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var planId = 999;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithZeroId_ShouldReturnNull()
    {
        // Arrange
        var planId = 0;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithNegativeId_ShouldReturnNull()
    {
        // Arrange
        var planId = -1;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var planId = 3;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None));

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var planId = 3;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _membershipPlanService.GetPlanByIdAsync(planId, cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetPlanByIdAsync_WithVariousIds_ShouldCallRepositoryWithCorrectId(int planId)
    {
        // Arrange
        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, f => f.PickRandom("Basic", "Pro", "Enterprise"))
            .RuleFor(p => p.IsActive, true)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PlanId.Should().Be(planId);

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithLargeNumberOfPlans_ShouldHandleCorrectly()
    {
        // Arrange
        var activePlans = new Faker<Plan>()
            .RuleFor(p => p.PlanId, f => f.Random.Int(1, 1000))
            .RuleFor(p => p.PlanName, f => f.PickRandom("Basic", "Pro", "Enterprise", "Premium", "Ultimate"))
            .RuleFor(p => p.PriceMonthly, f => f.Random.Decimal(9.99m, 999.99m))
            .RuleFor(p => p.MaxMapsPerMonth, f => f.Random.Int(25, 10000))
            .RuleFor(p => p.ExportQuota, f => f.Random.Int(50, 5000))
            .RuleFor(p => p.MaxUsersPerOrg, f => f.Random.Int(5, 1000))
            .RuleFor(p => p.IsActive, true)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent())
            .Generate(100);

        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var result = await _membershipPlanService.GetActivePlansAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        result.Should().BeEquivalentTo(activePlans);

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithInactivePlan_ShouldReturnPlan()
    {
        // Arrange
        var planId = 5;
        var inactivePlan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Legacy")
            .RuleFor(p => p.PriceMonthly, 19.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 50)
            .RuleFor(p => p.ExportQuota, 100)
            .RuleFor(p => p.MaxUsersPerOrg, 10)
            .RuleFor(p => p.IsActive, false) // Inactive plan
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent())
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactivePlan);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(inactivePlan);
        result!.PlanId.Should().Be(planId);
        result.IsActive.Should().BeFalse();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithMixedActiveInactivePlans_ShouldReturnOnlyActivePlans()
    {
        // Arrange
        var mixedPlans = new List<Plan>
        {
            new Faker<Plan>()
                .RuleFor(p => p.PlanId, 1)
                .RuleFor(p => p.PlanName, "Basic")
                .RuleFor(p => p.IsActive, true)
                .Generate(),
            new Faker<Plan>()
                .RuleFor(p => p.PlanId, 2)
                .RuleFor(p => p.PlanName, "Pro")
                .RuleFor(p => p.IsActive, true)
                .Generate(),
            new Faker<Plan>()
                .RuleFor(p => p.PlanId, 3)
                .RuleFor(p => p.PlanName, "Legacy")
                .RuleFor(p => p.IsActive, false) // Inactive plan
                .Generate()
        };

        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mixedPlans.Where(p => p.IsActive).ToList());

        // Act
        var result = await _membershipPlanService.GetActivePlansAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithUnlimitedPlan_ShouldReturnPlan()
    {
        // Arrange
        var planId = 4;
        var unlimitedPlan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Enterprise")
            .RuleFor(p => p.PriceMonthly, 99.99m)
            .RuleFor(p => p.MaxMapsPerMonth, -1) // Unlimited
            .RuleFor(p => p.ExportQuota, -1) // Unlimited
            .RuleFor(p => p.MaxUsersPerOrg, -1) // Unlimited
            .RuleFor(p => p.IsActive, true)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent())
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unlimitedPlan);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(unlimitedPlan);
        result!.PlanId.Should().Be(planId);
        result.MaxMapsPerMonth.Should().Be(-1);
        result.ExportQuota.Should().Be(-1);
        result.MaxUsersPerOrg.Should().Be(-1);

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlansAsync_WithNullReturnFromRepository_ShouldReturnEmptyList()
    {
        // Arrange
        _mockMembershipPlanRepository.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Plan>?)null);

        // Act
        var result = await _membershipPlanService.GetActivePlansAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockMembershipPlanRepository.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithMaxIntId_ShouldHandleCorrectly()
    {
        // Arrange
        var planId = int.MaxValue;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _membershipPlanService.GetPlanByIdAsync(planId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
