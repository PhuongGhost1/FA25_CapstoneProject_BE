using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
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
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly MembershipService _membershipService;
    private readonly Faker _faker;

    public MembershipServiceTests()
    {
        _mockMembershipRepository = new Mock<IMembershipRepository>();
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _membershipService = new MembershipService(_mockMembershipRepository.Object, _mockMembershipPlanRepository.Object, _mockOrganizationRepository.Object);
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
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, 3) // Pro plan
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .RuleFor(p => p.MaxMapsPerMonth, 100)
            .RuleFor(p => p.ExportQuota, 200)
            .RuleFor(p => p.MaxUsersPerOrg, 20)
            .RuleFor(p => p.DurationMonths, 1)
            .RuleFor(p => p.IsActive, true)
            .Generate();

        var currentMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, 2) // Basic plan
            .RuleFor(m => m.StartDate, DateTime.UtcNow.AddDays(-15))
            .RuleFor(m => m.EndDate, DateTime.UtcNow.AddDays(15))
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

    #region CreateOrRenewMembershipAsync Tests

    [Fact]
    public async Task CreateOrRenewMembershipAsync_WithNewMembership_ShouldCreateMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 1;
        var autoRenew = true;

        var plan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var newMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, planId)
            .RuleFor(m => m.StartDate, DateTime.UtcNow)
            .RuleFor(m => m.EndDate, DateTime.UtcNow.AddMonths(1))
            .Generate();

        var newUsage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, newMembership.MembershipId)
            .RuleFor(u => u.OrgId, orgId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Membership?)null);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newMembership);

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUsage);

        // Act
        var result = await _membershipService.CreateOrRenewMembershipAsync(userId, orgId, planId, autoRenew, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().PlanId.Should().Be(planId);
        _mockMembershipRepository.Verify(x => x.UpsertAsync(It.Is<DomainMembership.Membership>(m =>
            m.UserId == userId && m.OrgId == orgId && m.PlanId == planId), It.IsAny<CancellationToken>()), Times.Once);
        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrRenewMembershipAsync_WithSamePlan_ShouldExtendMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 1;
        var autoRenew = true;

        var plan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var existingMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, planId)
            .RuleFor(m => m.StartDate, DateTime.UtcNow.AddMonths(-1))
            .RuleFor(m => m.EndDate, DateTime.UtcNow)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMembership);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockMembershipRepository.Setup(x => x.UpsertAsync(It.IsAny<DomainMembership.Membership>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMembership);

        // Act
        var result = await _membershipService.CreateOrRenewMembershipAsync(userId, orgId, planId, autoRenew, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        existingMembership.EndDate.Should().BeAfter(DateTime.UtcNow);
        existingMembership.AutoRenew.Should().Be(autoRenew);
    }

    [Fact]
    public async Task CreateOrRenewMembershipAsync_WithNonExistentPlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var planId = 999;

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Plan?)null);

        // Act
        var result = await _membershipService.CreateOrRenewMembershipAsync(userId, orgId, planId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task CreateOrRenewMembershipAsync_WithDowngradeTooEarly_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var currentPlanId = 3;
        var newPlanId = 2;

        var currentPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, currentPlanId)
            .RuleFor(p => p.PriceMonthly, 29.99m)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var newPlan = new Faker<DomainMembership.Plan>()
            .RuleFor(p => p.PlanId, newPlanId)
            .RuleFor(p => p.PriceMonthly, 9.99m)
            .RuleFor(p => p.DurationMonths, 1)
            .Generate();

        var existingMembership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.PlanId, currentPlanId)
            .RuleFor(m => m.EndDate, DateTime.UtcNow.AddDays(30)) // 30 days remaining
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMembership);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(currentPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPlan);

        // Act
        var result = await _membershipService.CreateOrRenewMembershipAsync(userId, orgId, newPlanId, true, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    #endregion

    #region GetMembershipAsync Tests

    [Fact]
    public async Task GetMembershipAsync_WithValidId_ShouldReturnMembership()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var membership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.MembershipId, membershipId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByIdAsync(membershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        // Act
        var result = await _membershipService.GetMembershipAsync(membershipId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().MembershipId.Should().Be(membershipId);
    }

    [Fact]
    public async Task GetMembershipAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var membershipId = Guid.NewGuid();

        _mockMembershipRepository.Setup(x => x.GetByIdAsync(membershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Membership?)null);

        // Act
        var result = await _membershipService.GetMembershipAsync(membershipId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetMembershipByUserOrgAsync Tests

    [Fact]
    public async Task GetMembershipByUserOrgAsync_WithValidUserOrg_ShouldReturnMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        // Act
        var result = await _membershipService.GetMembershipByUserOrgAsync(userId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().UserId.Should().Be(userId);
        result.ValueOrFailure().OrgId.Should().Be(orgId);
    }

    [Fact]
    public async Task GetMembershipByUserOrgAsync_WithNonExistentMembership_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Membership?)null);

        // Act
        var result = await _membershipService.GetMembershipByUserOrgAsync(userId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetOrCreateUsageAsync Tests

    [Fact]
    public async Task GetOrCreateUsageAsync_WithExistingUsage_ShouldReturnUsage()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.GetOrCreateUsageAsync(membershipId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().MembershipId.Should().Be(membershipId);
        result.ValueOrFailure().OrgId.Should().Be(orgId);
    }

    [Fact]
    public async Task GetOrCreateUsageAsync_WithNonExistentUsage_ShouldCreateUsage()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.MembershipUsage?)null);

        var newUsage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.MapsCreatedThisCycle, 0)
            .RuleFor(u => u.ExportsThisCycle, 0)
            .RuleFor(u => u.ActiveUsersInOrg, 0)
            .Generate();

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUsage);

        // Act
        var result = await _membershipService.GetOrCreateUsageAsync(membershipId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(It.Is<DomainMembership.MembershipUsage>(u =>
            u.MembershipId == membershipId && u.OrgId == orgId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region TryConsumeQuotaAsync Tests

    [Fact]
    public async Task TryConsumeQuotaAsync_WithValidResourceKey_ShouldConsumeQuota()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.MapsCreatedThisCycle, 5)
            .RuleFor(u => u.ExportsThisCycle, 3)
            .RuleFor(u => u.ActiveUsersInOrg, 2)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        _mockMembershipRepository.Setup(x => x.UpsertUsageAsync(It.IsAny<DomainMembership.MembershipUsage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.TryConsumeQuotaAsync(membershipId, orgId, "maps", 1, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        usage.MapsCreatedThisCycle.Should().Be(6);
        _mockMembershipRepository.Verify(x => x.UpsertUsageAsync(usage, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TryConsumeQuotaAsync_WithInvalidResourceKey_ShouldReturnError()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.TryConsumeQuotaAsync(membershipId, orgId, "invalid", 1, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    #endregion

    #region ResetUsageCycleAsync Tests

    [Fact]
    public async Task ResetUsageCycleAsync_ShouldSucceed()
    {
        // Arrange
        var membershipId = Guid.NewGuid();

        // Act
        var result = await _membershipService.ResetUsageCycleAsync(membershipId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    #endregion

    #region HasFeatureAsync Tests

    [Fact]
    public async Task HasFeatureAsync_WithFeatureEnabled_ShouldReturnTrue()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "advanced_export";
        var featureFlags = "{\"advanced_export\": true}";

        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.FeatureFlags, featureFlags)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.HasFeatureAsync(membershipId, orgId, featureKey, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    [Fact]
    public async Task HasFeatureAsync_WithFeatureDisabled_ShouldReturnFalse()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "advanced_export";
        var featureFlags = "{\"advanced_export\": false}";

        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.FeatureFlags, featureFlags)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.HasFeatureAsync(membershipId, orgId, featureKey, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeFalse();
    }

    [Fact]
    public async Task HasFeatureAsync_WithNoFeatureFlags_ShouldReturnFalse()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "advanced_export";

        var usage = new Faker<DomainMembership.MembershipUsage>()
            .RuleFor(u => u.UsageId, Guid.NewGuid())
            .RuleFor(u => u.MembershipId, membershipId)
            .RuleFor(u => u.OrgId, orgId)
            .RuleFor(u => u.FeatureFlags, (string?)null)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usage);

        // Act
        var result = await _membershipService.HasFeatureAsync(membershipId, orgId, featureKey, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeFalse();
    }

    #endregion

    #region GetCurrentMembershipWithIncludesAsync Tests

    [Fact]
    public async Task GetCurrentMembershipWithIncludesAsync_WithValidUserOrg_ShouldReturnMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membership = new Faker<DomainMembership.Membership>()
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .Generate();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        // Act
        var result = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().UserId.Should().Be(userId);
        result.ValueOrFailure().OrgId.Should().Be(orgId);
    }

    [Fact]
    public async Task GetCurrentMembershipWithIncludesAsync_WithNonExistentMembership_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMembership.Membership?)null);

        // Act
        var result = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task GetCurrentMembershipWithIncludesAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _mockMembershipRepository.Setup(x => x.GetByUserOrgWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion
}
