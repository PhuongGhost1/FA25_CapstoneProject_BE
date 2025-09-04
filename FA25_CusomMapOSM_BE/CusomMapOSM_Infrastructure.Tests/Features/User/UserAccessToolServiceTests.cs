using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.AccessTools;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Features.User;
using FluentAssertions;
using Moq;
using Optional;
using System.Text.Json;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.User;

public class UserAccessToolServiceTests
{
    private readonly Mock<IUserAccessToolRepository> _mockUserAccessToolRepository;
    private readonly Mock<IAccessToolRepository> _mockAccessToolRepository;
    private readonly Mock<IMembershipPlanRepository> _mockMembershipPlanRepository;
    private readonly UserAccessToolService _userAccessToolService;
    private readonly Faker _faker;

    public UserAccessToolServiceTests()
    {
        _mockUserAccessToolRepository = new Mock<IUserAccessToolRepository>();
        _mockAccessToolRepository = new Mock<IAccessToolRepository>();
        _mockMembershipPlanRepository = new Mock<IMembershipPlanRepository>();

        _userAccessToolService = new UserAccessToolService(
            _mockUserAccessToolRepository.Object,
            _mockAccessToolRepository.Object,
            _mockMembershipPlanRepository.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task GetUserAccessToolsAsync_WithValidUserId_ShouldReturnAccessTools()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .RuleFor(uat => uat.GrantedAt, f => f.Date.Past())
            .RuleFor(uat => uat.ExpiredAt, f => f.Date.Future())
            .Generate(3);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAccessTools);

        // Act
        var result = await _userAccessToolService.GetUserAccessToolsAsync(userId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().HaveCount(3);
        result.ValueOrFailure().Should().BeEquivalentTo(userAccessTools);

        _mockUserAccessToolRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserAccessToolsAsync_WithNoAccessTools_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyAccessTools = new List<UserAccessTool>();

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyAccessTools);

        // Act
        var result = await _userAccessToolService.GetUserAccessToolsAsync(userId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeEmpty();

        _mockUserAccessToolRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserAccessToolsAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _userAccessToolService.GetUserAccessToolsAsync(userId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("UserAccessTool.GetFailed");
            }
        );
    }

    [Fact]
    public async Task GetActiveUserAccessToolsAsync_WithValidUserId_ShouldReturnActiveAccessTools()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .RuleFor(uat => uat.GrantedAt, f => f.Date.Past())
            .RuleFor(uat => uat.ExpiredAt, f => f.Date.Future())
            .Generate(2);

        _mockUserAccessToolRepository.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAccessTools);

        // Act
        var result = await _userAccessToolService.GetActiveUserAccessToolsAsync(userId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().HaveCount(2);
        result.ValueOrFailure().Should().BeEquivalentTo(activeAccessTools);

        _mockUserAccessToolRepository.Verify(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasAccessToToolAsync_WithValidAccess_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        _mockUserAccessToolRepository.Setup(x => x.HasAccessAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userAccessToolService.HasAccessToToolAsync(userId, accessToolId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockUserAccessToolRepository.Verify(x => x.HasAccessAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasAccessToToolAsync_WithNoAccess_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        _mockUserAccessToolRepository.Setup(x => x.HasAccessAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userAccessToolService.HasAccessToToolAsync(userId, accessToolId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeFalse();

        _mockUserAccessToolRepository.Verify(x => x.HasAccessAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantAccessToToolAsync_WithNewAccess_ShouldCreateNewAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var accessTool = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, accessToolId)
            .RuleFor(at => at.AccessToolName, "Test Tool")
            .RuleFor(at => at.RequiredMembership, true)
            .Generate();

        var createdUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, accessToolId)
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, expiredAt)
            .Generate();

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessTool);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserAccessTool);

        // Act
        var result = await _userAccessToolService.GrantAccessToToolAsync(userId, accessToolId, expiredAt, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeEquivalentTo(createdUserAccessTool);

        _mockAccessToolRepository.Verify(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.GetByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantAccessToToolAsync_WithExistingAccess_ShouldUpdateExistingAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var accessTool = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, accessToolId)
            .RuleFor(at => at.AccessToolName, "Test Tool")
            .RuleFor(at => at.RequiredMembership, true)
            .Generate();

        var existingUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, accessToolId)
            .RuleFor(uat => uat.GrantedAt, f => f.Date.Past())
            .RuleFor(uat => uat.ExpiredAt, f => f.Date.Past())
            .Generate();

        var updatedUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, existingUserAccessTool.UserAccessToolId)
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, accessToolId)
            .RuleFor(uat => uat.GrantedAt, existingUserAccessTool.GrantedAt)
            .RuleFor(uat => uat.ExpiredAt, expiredAt)
            .Generate();

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessTool);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserAccessTool);

        _mockUserAccessToolRepository.Setup(x => x.UpdateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUserAccessTool);

        // Act
        var result = await _userAccessToolService.GrantAccessToToolAsync(userId, accessToolId, expiredAt, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeEquivalentTo(updatedUserAccessTool);

        _mockAccessToolRepository.Verify(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.GetByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.UpdateAsync(It.Is<UserAccessTool>(uat => uat.ExpiredAt == expiredAt), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantAccessToToolAsync_WithNonExistentAccessTool_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 999;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessTool?)null);

        // Act
        var result = await _userAccessToolService.GrantAccessToToolAsync(userId, accessToolId, expiredAt, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.NotFound);
                error.Code.Should().Be("UserAccessTool.AccessToolNotFound");
            }
        );

        _mockAccessToolRepository.Verify(x => x.GetByIdAsync(accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAccessToToolAsync_WithValidAccess_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        _mockUserAccessToolRepository.Setup(x => x.DeleteByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userAccessToolService.RevokeAccessToToolAsync(userId, accessToolId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockUserAccessToolRepository.Verify(x => x.DeleteByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAccessToToolAsync_WithNoAccess_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        _mockUserAccessToolRepository.Setup(x => x.DeleteByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userAccessToolService.RevokeAccessToToolAsync(userId, accessToolId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeFalse();

        _mockUserAccessToolRepository.Verify(x => x.DeleteByUserAndToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantAccessToToolsAsync_WithValidToolIds_ShouldGrantAccessToAllTools()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolIds = new List<int> { 1, 2, 3 };
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var accessTool = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, f => f.PickRandom(accessToolIds))
            .RuleFor(at => at.AccessToolName, "Test Tool")
            .RuleFor(at => at.RequiredMembership, true)
            .Generate();

        var createdUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.PickRandom(accessToolIds))
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, expiredAt)
            .Generate();

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessTool);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserAccessTool);

        // Act
        var result = await _userAccessToolService.GrantAccessToToolsAsync(userId, accessToolIds, expiredAt, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockAccessToolRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _mockUserAccessToolRepository.Verify(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GrantAccessToToolsAsync_WithOneToolFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToolIds = new List<int> { 1, 2, 3 };
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var accessTool = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, 1)
            .RuleFor(at => at.AccessToolName, "Test Tool")
            .RuleFor(at => at.RequiredMembership, true)
            .Generate();

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessTool);

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccessTool
            {
                UserId = Guid.NewGuid(),
                AccessToolId = 1,
                UserAccessToolId = 0,
                GrantedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(30)
            });

        // Act
        var result = await _userAccessToolService.GrantAccessToToolsAsync(userId, accessToolIds, expiredAt, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("UserAccessTool.GrantAccessFailed");
            }
        );
    }

    [Fact]
    public async Task RevokeAllAccessToolsAsync_WithValidUserId_ShouldRevokeAllAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .Generate(3);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAccessTools);

        _mockUserAccessToolRepository.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userAccessToolService.RevokeAllAccessToolsAsync(userId, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockUserAccessToolRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task UpdateAccessToolsForMembershipAsync_WithValidPlan_ShouldUpdateAccessTools()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.AccessToolIds, "[1,2,3]")
            .Generate();

        var accessTool = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, f => f.Random.Int(1, 3))
            .RuleFor(at => at.AccessToolName, "Test Tool")
            .RuleFor(at => at.RequiredMembership, true)
            .Generate();

        var userAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .Generate(2);

        var createdUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 3))
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, membershipExpiryDate)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessTool);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAccessTools);

        _mockUserAccessToolRepository.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserAccessTool);

        // Act
        var result = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUserAccessToolRepository.Verify(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task UpdateAccessToolsForMembershipAsync_WithNonExistentPlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = 999;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var result = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.NotFound);
                error.Code.Should().Be("UserAccessTool.PlanNotFound");
            }
        );

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccessToolsForMembershipAsync_WithInvalidJsonInPlan_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.AccessToolIds, "invalid json")
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Validation);
                error.Code.Should().Be("UserAccessTool.InvalidAccessToolIds");
            }
        );

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccessToolsForMembershipAsync_WithEmptyAccessToolIds_ShouldGrantFreeTools()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.AccessToolIds, "")
            .Generate();

        var freeTools = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, f => f.Random.Int(1, 5))
            .RuleFor(at => at.AccessToolName, "Free Tool")
            .RuleFor(at => at.RequiredMembership, false)
            .Generate(3);

        var userAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .Generate(2);

        var createdUserAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 5))
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, membershipExpiryDate)
            .Generate();

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockAccessToolRepository.Setup(x => x.GetByRequiredMembershipAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeTools);

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeTools.First());

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAccessTools);

        _mockUserAccessToolRepository.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserAccessTool);

        // Act
        var result = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockMembershipPlanRepository.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _mockAccessToolRepository.Verify(x => x.GetByRequiredMembershipAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessToolRepository.Verify(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Theory]
    [InlineData("")]
    [InlineData("")]
    [InlineData("null")]
    public async Task UpdateAccessToolsForMembershipAsync_WithNullOrEmptyAccessToolIds_ShouldGrantFreeTools(string accessToolIds)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.AccessToolIds, accessToolIds)
            .Generate();

        var freeTools = new Faker<AccessTool>()
            .RuleFor(at => at.AccessToolId, f => f.Random.Int(1, 5))
            .RuleFor(at => at.AccessToolName, "Free Tool")
            .RuleFor(at => at.RequiredMembership, false)
            .Generate(2);

        _mockMembershipPlanRepository.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mockAccessToolRepository.Setup(x => x.GetByRequiredMembershipAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeTools);

        _mockAccessToolRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeTools.First());

        _mockUserAccessToolRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAccessTool>());

        _mockUserAccessToolRepository.Setup(x => x.GetByUserAndToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccessTool?)null);

        _mockUserAccessToolRepository.Setup(x => x.CreateAsync(It.IsAny<UserAccessTool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccessTool
            {
                UserId = Guid.NewGuid(),
                AccessToolId = 1,
                UserAccessToolId = 0,
                GrantedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(30)
            });

        // Act
        var result = await _userAccessToolService.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();

        _mockAccessToolRepository.Verify(x => x.GetByRequiredMembershipAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
