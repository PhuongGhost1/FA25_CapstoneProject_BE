using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Features.Membership;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Membership;

public class MembershipServiceTests
{
    private readonly Mock<IMembershipRepository> _mockMembershipRepository;
    private readonly Mock<IMembershipPlanRepository> _mockMembershipPlanRepository;
    private readonly MembershipService _membershipService;
    private readonly Faker _faker;

    public MembershipServiceTests()
    {
        _mockMembershipRepository = new Mock<IMembershipRepository>();
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();
        _membershipService = new MembershipService(_mockMembershipRepository.Object, _mockMembershipPlanRepository.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithValidUpgrade_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 3; // Pro plan
        var autoRenew = true;

        var currentPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 2) // Basic plan
            .RuleFor(p => p.PlanName, "Basic")
            .RuleFor(p => p.PriceMonthly, 9.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 25)
            .RuleFor(p => p.ExportQuota, 50)
            .RuleFor(p => p.MaxUsersPerOrg, 5)
            .Generate();

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 3) // Pro plan
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 100)
            .RuleFor(p => p.ExportQuota, 200)
            .RuleFor(p => p.MaxUsersPerOrg, 20)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, 2) // Basic plan
            .RuleFor(m => m.StartDate, DateTime.UtcNow.AddDays(-15))
            .RuleFor(m => m.AutoRenew, true)
            .Generate();

        var currentUsage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, currentMembership.MembershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.MapsCreatedThisCycle, 15)
            .RuleFor(u => u.ExportsThisCycle, 20)
            .RuleFor(u => u.ActiveUsersInOrg, 3)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(currentMembership.PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPlan);

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(currentMembership.MembershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, autoRenew, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().PlanId.Should().Be(newPlanId);
        result.ValueOrFailure().AutoRenew.Should().Be(autoRenew);

        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(It.Is<DomainMembership.MembershipUsage>(u =>
            u.MapsCreatedThisCycle == 0 &&
            u.ExportsThisCycle == 0 &&
            u.ActiveUsersInOrg == 0), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithValidDowngrade_ShouldCapUsage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 2; // Basic plan
        var autoRenew = false;

        var currentPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 3) // Pro plan
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 100)
            .RuleFor(p => p.ExportQuota, 200)
            .RuleFor(p => p.MaxUsersPerOrg, 20)
            .Generate();

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 2) // Basic plan
            .RuleFor(p => p.PlanName, "Basic")
            .RuleFor(p => p.PriceMonthly, 9.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 25)
            .RuleFor(p => p.ExportQuota, 50)
            .RuleFor(p => p.MaxUsersPerOrg, 5)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, 3) // Pro plan
            .RuleFor(m => m.StartDate, DateTime.UtcNow.AddDays(-15))
            .RuleFor(m => m.AutoRenew, true)
            .Generate();

        var currentUsage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, currentMembership.MembershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.MapsCreatedThisCycle, 80) // Exceeds Basic plan limit
            .RuleFor(u => u.ExportsThisCycle, 150) // Exceeds Basic plan limit
            .RuleFor(u => u.ActiveUsersInOrg, 10) // Exceeds Basic plan limit
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(currentMembership.PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPlan);

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(currentMembership.MembershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, autoRenew, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().PlanId.Should().Be(newPlanId);
        result.ValueOrFailure().AutoRenew.Should().Be(autoRenew);

        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(It.Is<DomainMembership.MembershipUsage>(u =>
            u.MapsCreatedThisCycle == 25 &&
            u.ExportsThisCycle == 50 &&
            u.ActiveUsersInOrg == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithNonExistentPlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 999; // Non-existent plan

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Plan?)null);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithNoMembership_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 3;

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, newPlanId)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Membership?)null);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithSamePlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 3; // Same plan

        var plan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, planId) // Same plan
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, planId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithInactivePlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 3;

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, newPlanId)
            .RuleFor(p => p.IsActive, false) // Inactive plan
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 3;

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, newPlanId)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, 2)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task ChangeSubscriptionPlanAsync_WithUnlimitedPlan_ShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var newPlanId = 4; // Enterprise plan (unlimited)

        var currentPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 3) // Pro plan
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .Generate();

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 4) // Enterprise plan
            .RuleFor(p => p.PriceMonthly, 99.99m)
            .RuleFor(p => p.MaxMapsPerMonth, -1) // Unlimited
            .RuleFor(p => p.ExportQuota, -1) // Unlimited
            .RuleFor(p => p.MaxUsersPerOrg, -1) // Unlimited
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, 3)
            .Generate();

        var currentUsage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, currentMembership.MembershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.MapsCreatedThisCycle, 50)
            .RuleFor(u => u.ExportsThisCycle, 100)
            .RuleFor(u => u.ActiveUsersInOrg, 15)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(currentMembership.PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPlan);

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(currentMembership.MembershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUsage);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentMembership);

        // Act
        var result = await _membershipService.ChangeSubscriptionPlanAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().PlanId.Should().Be(newPlanId);

        // Should reset usage for upgrade to unlimited plan
        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(It.Is<DomainMembership.MembershipUsage>(u =>
            u.MapsCreatedThisCycle == 0 &&
            u.ExportsThisCycle == 0 &&
            u.ActiveUsersInOrg == 0), It.IsAny<CancellationToken>()), Times.Once);
    }
}
