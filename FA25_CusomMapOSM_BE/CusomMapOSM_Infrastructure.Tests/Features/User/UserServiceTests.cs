using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Models.DTOs.Features.User;
using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Features.User;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.User;

public class UserServiceTests
{
    private readonly Mock<IAuthenticationRepository> _mockRepository;
    private readonly UserService _userService;
    private readonly Faker _faker;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IAuthenticationRepository>();
        _userService = new UserService(_mockRepository.Object);
        _faker = new Faker();
    }

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .RuleFor(u => u.CreatedAt, DateTime.UtcNow)
            .Generate();

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.UserId.Should().Be(userId);
        response.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync((DomainUser.User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.NotFound);
                error.Code.Should().Be("User.NotFound");
            }
        );
    }

    [Fact]
    public async Task GetUserByIdAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetUserById(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("User.GetFailed");
            }
        );
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var user = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, email)
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .Generate();

        _mockRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByEmailAsync(email, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnError()
    {
        // Arrange
        var email = _faker.Internet.Email();

        _mockRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((DomainUser.User?)null);

        // Act
        var result = await _userService.GetUserByEmailAsync(email, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.NotFound);
                error.Code.Should().Be("User.NotFound");
            }
        );
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var email = _faker.Internet.Email();

        _mockRepository.Setup(x => x.GetUserByEmail(email))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.GetUserByEmailAsync(email, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("User.GetFailed");
            }
        );
    }

    #endregion

    #region UpdateUserPersonalInfoAsync Tests

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithValidFullName_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, "Old Name")
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .RuleFor(u => u.CreatedAt, DateTime.UtcNow)
            .Generate();

        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "New Name",
            Phone = null
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(x => x.UpdateUser(existingUser)).ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        response.User.FullName.Should().Be("New Name");
        existingUser.FullName.Should().Be("New Name");
        _mockRepository.Verify(x => x.UpdateUser(existingUser), Times.Once);
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithValidPhone_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Phone, "1234567890")
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .RuleFor(u => u.CreatedAt, DateTime.UtcNow)
            .Generate();

        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = null,
            Phone = "9876543210"
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(x => x.UpdateUser(existingUser)).ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.User.Phone.Should().Be("9876543210");
        existingUser.Phone.Should().Be("9876543210");
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithBothFields_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, "Old Name")
            .RuleFor(u => u.Phone, "1234567890")
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .RuleFor(u => u.CreatedAt, DateTime.UtcNow)
            .Generate();

        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "New Name",
            Phone = "9876543210"
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(x => x.UpdateUser(existingUser)).ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.User.FullName.Should().Be("New Name");
        response.User.Phone.Should().Be("9876543210");
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithWhitespaceFields_ShouldTrim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, "Old Name")
            .RuleFor(u => u.Phone, "1234567890")
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .RuleFor(u => u.CreatedAt, DateTime.UtcNow)
            .Generate();

        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "  Trimmed Name  ",
            Phone = "  9876543210  "
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(x => x.UpdateUser(existingUser)).ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        existingUser.FullName.Should().Be("Trimmed Name");
        existingUser.Phone.Should().Be("9876543210");
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithEmptyFields_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = null,
            Phone = null
        };

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Validation);
                error.Code.Should().Be("User.UpdateValidation");
            }
        );
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithWhitespaceOnlyFields_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "   ",
            Phone = "   "
        };

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Validation);
                error.Code.Should().Be("User.UpdateValidation");
            }
        );
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "New Name"
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync((DomainUser.User?)null);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.NotFound);
                error.Code.Should().Be("User.NotFound");
            }
        );
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithUpdateFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, "Old Name")
            .RuleFor(u => u.Role, UserRoleEnum.RegisteredUser)
            .RuleFor(u => u.AccountStatus, AccountStatusEnum.Active)
            .Generate();

        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "New Name"
        };

        _mockRepository.Setup(x => x.GetUserById(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(x => x.UpdateUser(existingUser)).ReturnsAsync(false);

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("User.UpdateFailed");
            }
        );
    }

    [Fact]
    public async Task UpdateUserPersonalInfoAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserPersonalInfoRequest
        {
            FullName = "New Name"
        };

        _mockRepository.Setup(x => x.GetUserById(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.UpdateUserPersonalInfoAsync(userId, request, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Failure);
                error.Code.Should().Be("User.UpdateFailed");
            }
        );
    }

    #endregion
}

