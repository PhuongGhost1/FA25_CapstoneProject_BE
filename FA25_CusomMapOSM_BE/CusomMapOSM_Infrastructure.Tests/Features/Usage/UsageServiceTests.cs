using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using DomainOrganization = CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Features.Usage;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;
using CusomMapOSM_Infrastructure.Services;

namespace CusomMapOSM_Infrastructure.Tests.Features.Usage;

public class UsageServiceTests
{
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<IEmailNotificationService> _mockNotificationService;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly UsageService _usageService;
    private readonly Faker _faker;

    public UsageServiceTests()
    {
        _mockMembershipService = new Mock<IMembershipService>();
        _mockNotificationService = new Mock<IEmailNotificationService>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _usageService = new UsageService(
            _mockMembershipService.Object,
            _mockNotificationService.Object,
            _mockOrganizationRepository.Object
        );
        _faker = new Faker();
    }

    #region GetUserUsageAsync Tests

    [Fact]
    public async Task GetUserUsageAsync_WithValidData_ShouldReturnUsage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
        var planId = 1;

        var plan = new DomainMembership.Plan
        {
            PlanId = planId,
            PlanName = "Basic Plan",
            MapQuota = 10,
            ExportQuota = 5,
            MaxUsersPerOrg = 3,
            MaxCustomLayers = 2,
            MonthlyTokens = 10000
        };

        var membership = new DomainMembership.Membership
        {
            MembershipId = membershipId,
            UserId = userId,
            OrgId = orgId,
            PlanId = planId,
            Plan = plan
        };

        var usage = new DomainMembership.MembershipUsage
        {
            UsageId = Guid.NewGuid(),
            MembershipId = membershipId,
            OrgId = orgId,
            MapsCreatedThisCycle = 5,
            ExportsThisCycle = 2,
            ActiveUsersInOrg = 1,
            CycleStartDate = DateTime.UtcNow.AddDays(-15),
            CycleEndDate = DateTime.UtcNow.AddDays(15)
        };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.GetUserUsageAsync(userId, orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.UserId.Should().Be(userId);
        response.MembershipId.Should().Be(membershipId);
        response.PlanName.Should().Be("Basic Plan");
        response.Quotas.Should().NotBeEmpty();
        response.Quotas.Should().Contain(q => q.ResourceType == "maps");
    }

    [Fact]
    public async Task GetUserUsageAsync_WithNoMembership_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership.Membership, Error>(Error.NotFound("Membership.NotFound", "No active membership found")));

        // Act
        var result = await _usageService.GetUserUsageAsync(userId, orgId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task GetUserUsageAsync_WithUsageCreationFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var membership = new DomainMembership.Membership { MembershipId = membershipId, UserId = userId, OrgId = orgId };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership.MembershipUsage, Error>(Error.Failure("Usage.GetFailed", "Failed to get usage")));

        // Act
        var result = await _usageService.GetUserUsageAsync(userId, orgId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CheckUserQuotaAsync Tests

    [Fact]
    public async Task CheckUserQuotaAsync_WithAvailableQuota_ShouldReturnAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var plan = new DomainMembership.Plan { MapQuota = 10 };
        var membership = new DomainMembership.Membership { MembershipId = membershipId, Plan = plan };
        var usage = new DomainMembership.MembershipUsage { MapsCreatedThisCycle = 5 };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.CheckUserQuotaAsync(userId, orgId, "maps", 3);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.IsAllowed.Should().BeTrue();
        response.CurrentUsage.Should().Be(5);
        response.Limit.Should().Be(10);
        // RemainingQuota = Limit - CurrentUsage = 10 - 5 = 5 (not accounting for requested amount)
        response.RemainingQuota.Should().Be(5);
    }

    [Fact]
    public async Task CheckUserQuotaAsync_WithExceededQuota_ShouldReturnNotAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var plan = new DomainMembership.Plan { MapQuota = 10 };
        var membership = new DomainMembership.Membership { MembershipId = membershipId, Plan = plan };
        var usage = new DomainMembership.MembershipUsage { MapsCreatedThisCycle = 8 };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.CheckUserQuotaAsync(userId, orgId, "maps", 5);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.IsAllowed.Should().BeFalse();
        response.RemainingQuota.Should().Be(2);
    }

    [Fact]
    public async Task CheckUserQuotaAsync_WithUnlimitedQuota_ShouldReturnAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var plan = new DomainMembership.Plan { MapQuota = -1 }; // Unlimited
        var membership = new DomainMembership.Membership { MembershipId = membershipId, Plan = plan };
        var usage = new DomainMembership.MembershipUsage { MapsCreatedThisCycle = 1000 };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.CheckUserQuotaAsync(userId, orgId, "maps", 10000);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.IsAllowed.Should().BeTrue();
        response.RemainingQuota.Should().Be(int.MaxValue);
    }

    #endregion

    #region ConsumeUserQuotaAsync Tests

    [Fact]
    public async Task ConsumeUserQuotaAsync_WithValidConsumption_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var membership = new DomainMembership.Membership { MembershipId = membershipId };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(membershipId, orgId, "maps", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _usageService.ConsumeUserQuotaAsync(userId, orgId, "maps", 1);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    [Fact]
    public async Task ConsumeUserQuotaAsync_WithQuotaExceeded_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var membership = new DomainMembership.Membership { MembershipId = membershipId };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(membershipId, orgId, "maps", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(false));

        // Act
        var result = await _usageService.ConsumeUserQuotaAsync(userId, orgId, "maps", 1);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ConsumeUserQuotaAsync_WithConsumptionFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var membership = new DomainMembership.Membership { MembershipId = membershipId };

        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(membershipId, orgId, "maps", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<bool, Error>(Error.Failure("ConsumeFailed", "Failed")));

        // Act
        var result = await _usageService.ConsumeUserQuotaAsync(userId, orgId, "maps", 1);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region GetOrganizationUsageAsync Tests

    [Fact]
    public async Task GetOrganizationUsageAsync_WithValidData_ShouldReturnUsage()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var organization = new DomainOrganization.Organization
        {
            OrgId = orgId,
            OrgName = "Test Org",
            OwnerUserId = ownerUserId
        };

        var plan = new DomainMembership.Plan { PlanName = "Pro Plan" };
        var membership = new DomainMembership.Membership { MembershipId = membershipId, Plan = plan };
        var usage = new DomainMembership.MembershipUsage
        {
            CycleStartDate = DateTime.UtcNow.AddDays(-15),
            CycleEndDate = DateTime.UtcNow.AddDays(15)
        };

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync(organization);
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(ownerUserId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.GetOrganizationUsageAsync(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.OrganizationId.Should().Be(orgId);
        response.OrganizationName.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetOrganizationUsageAsync_WithNonExistentOrg_ShouldReturnError()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync((DomainOrganization.Organization?)null);

        // Act
        var result = await _usageService.GetOrganizationUsageAsync(orgId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region CheckOrganizationQuotaAsync Tests

    [Fact]
    public async Task CheckOrganizationQuotaAsync_WithValidData_ShouldReturnQuota()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var organization = new DomainOrganization.Organization { OrgId = orgId, OwnerUserId = ownerUserId };
        var plan = new DomainMembership.Plan { MapQuota = 20 };
        var membership = new DomainMembership.Membership { MembershipId = membershipId, Plan = plan };
        var usage = new DomainMembership.MembershipUsage { MapsCreatedThisCycle = 10 };

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync(organization);
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(ownerUserId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetOrCreateUsageAsync(membershipId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.MembershipUsage, Error>(usage));

        // Act
        var result = await _usageService.CheckOrganizationQuotaAsync(orgId, "maps", 5);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.IsAllowed.Should().BeTrue();
    }

    #endregion

    #region ConsumeOrganizationQuotaAsync Tests

    [Fact]
    public async Task ConsumeOrganizationQuotaAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var organization = new DomainOrganization.Organization { OrgId = orgId, OwnerUserId = ownerUserId };
        var membership = new DomainMembership.Membership { MembershipId = membershipId };

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync(organization);
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(ownerUserId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership.Membership, Error>(membership));
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(membershipId, orgId, "maps", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _usageService.ConsumeOrganizationQuotaAsync(orgId, "maps", 1);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    #endregion

    #region ResetUsageCycleAsync Tests

    [Fact]
    public async Task ResetUsageCycleAsync_WithValidMembership_ShouldSucceed()
    {
        // Arrange
        var membershipId = Guid.NewGuid();

        _mockMembershipService.Setup(x => x.ResetUsageCycleAsync(membershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _usageService.ResetUsageCycleAsync(membershipId);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    [Fact]
    public async Task ResetUsageCycleAsync_WithFailure_ShouldReturnError()
    {
        // Arrange
        var membershipId = Guid.NewGuid();

        _mockMembershipService.Setup(x => x.ResetUsageCycleAsync(membershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<bool, Error>(Error.Failure("ResetFailed", "Failed")));

        // Act
        var result = await _usageService.ResetUsageCycleAsync(membershipId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CheckAndNotifyQuotaWarningsAsync Tests

    [Fact]
    public async Task CheckAndNotifyQuotaWarningsAsync_ShouldSucceed()
    {
        // Act
        var result = await _usageService.CheckAndNotifyQuotaWarningsAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    #endregion
}

