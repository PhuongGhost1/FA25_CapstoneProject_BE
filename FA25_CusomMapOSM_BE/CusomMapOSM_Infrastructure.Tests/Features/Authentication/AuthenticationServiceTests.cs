using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Response;
using CusomMapOSM_Application.Models.DTOs.Services;
using DomainUsers = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Features.Authentication;
using CusomMapOSM_Infrastructure.Services;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<IAuthenticationRepository> _mockAuthenticationRepository;
    private readonly Mock<ITypeRepository> _mockTypeRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IMailService> _mockMailService;
    private readonly Mock<IRedisCacheService> _mockRedisCacheService;
    private readonly Mock<HangfireEmailService> _mockHangfireEmailService;
    private readonly AuthenticationService _authenticationService;
    private readonly Faker _faker;

    public AuthenticationServiceTests()
    {
        _mockAuthenticationRepository = new Mock<IAuthenticationRepository>();
        _mockTypeRepository = new Mock<ITypeRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockMailService = new Mock<IMailService>();
        _mockRedisCacheService = new Mock<IRedisCacheService>();
        _mockHangfireEmailService = new Mock<HangfireEmailService>();

        _authenticationService = new AuthenticationService(
            _mockAuthenticationRepository.Object,
            _mockJwtService.Object,
            _mockRedisCacheService.Object,
            _mockTypeRepository.Object,
            _mockHangfireEmailService.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, request.Email)
            .RuleFor(u => u.PasswordHash, "hashed_password")
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.RoleId, Guid.NewGuid())
            .RuleFor(u => u.AccountStatusId, Guid.NewGuid())
            .Generate();

        var token = "valid_jwt_token";

        _mockJwtService.Setup(x => x.HashObject<string>(request.Password))
            .Returns("hashed_password");

        _mockAuthenticationRepository.Setup(x => x.Login(request.Email, "hashed_password"))
            .ReturnsAsync(user);

        _mockJwtService.Setup(x => x.GenerateToken(user.UserId, user.Email, It.IsAny<int>()))
            .Returns(token);

        // Act
        var result = await _authenticationService.Login(request);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Token.Should().Be(token);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, "")
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        // Act
        var result = await _authenticationService.Login(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, "")
            .Generate();

        // Act
        var result = await _authenticationService.Login(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        _mockJwtService.Setup(x => x.HashObject<string>(request.Password))
            .Returns("hashed_password");

        _mockAuthenticationRepository.Setup(x => x.Login(request.Email, "hashed_password"))
            .ReturnsAsync((DomainUsers.User?)null);

        // Act
        var result = await _authenticationService.Login(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Login_WithTokenGenerationFailure_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, request.Email)
            .RuleFor(u => u.PasswordHash, "hashed_password")
            .Generate();

        _mockJwtService.Setup(x => x.HashObject<string>(request.Password))
            .Returns("hashed_password");

        _mockAuthenticationRepository.Setup(x => x.Login(request.Email, "hashed_password"))
            .ReturnsAsync(user);

        _mockJwtService.Setup(x => x.GenerateToken(user.UserId, user.Email, It.IsAny<int>()))
            .Returns((string?)null);

        // Act
        var result = await _authenticationService.Login(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task LogOut_WithValidUserId_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.AccountStatusId, Guid.NewGuid())
            .Generate();

        var inactiveStatus = new Faker<DomainUsers.AccountStatus>()
            .RuleFor(s => s.StatusId, Guid.NewGuid())
            .RuleFor(s => s.Name, AccountStatusEnum.Inactive.ToString())
            .Generate();

        _mockAuthenticationRepository.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(user);

        _mockTypeRepository.Setup(x => x.GetAccountStatusById(AccountStatusEnum.Inactive))
            .ReturnsAsync(inactiveStatus);

        _mockAuthenticationRepository.Setup(x => x.UpdateUser(It.IsAny<DomainUsers.User>()))
            .ReturnsAsync(true);

        _mockRedisCacheService.Setup(x => x.ForceLogout(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authenticationService.LogOut(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Result.Should().Be("Logout successfully");

        _mockAuthenticationRepository.Verify(x => x.UpdateUser(It.Is<DomainUsers.User>(u =>
            u.AccountStatusId == inactiveStatus.StatusId)), Times.Once);
    }

    [Fact]
    public async Task LogOut_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockAuthenticationRepository.Setup(x => x.GetUserById(userId))
            .ReturnsAsync((DomainUsers.User?)null);

        // Act
        var result = await _authenticationService.LogOut(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task VerifyEmail_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        var userRole = new Faker<DomainUsers.UserRole>()
            .RuleFor(r => r.RoleId, Guid.NewGuid())
            .RuleFor(r => r.Name, UserRoleEnum.RegisteredUser.ToString())
            .Generate();

        var accountStatus = new Faker<DomainUsers.AccountStatus>()
            .RuleFor(s => s.StatusId, Guid.NewGuid())
            .RuleFor(s => s.Name, AccountStatusEnum.PendingVerification.ToString())
            .Generate();

        _mockAuthenticationRepository.Setup(x => x.IsEmailExists(request.Email))
            .ReturnsAsync(false);

        _mockTypeRepository.Setup(x => x.GetUserRoleById(UserRoleEnum.RegisteredUser))
            .ReturnsAsync(userRole);

        _mockTypeRepository.Setup(x => x.GetAccountStatusById(AccountStatusEnum.PendingVerification))
            .ReturnsAsync(accountStatus);

        _mockJwtService.Setup(x => x.HashObject<string>(request.Password))
            .Returns("hashed_password");

        _mockAuthenticationRepository.Setup(x => x.Register(It.IsAny<DomainUsers.User>()))
            .ReturnsAsync(true);

        _mockRedisCacheService.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        _mockMailService.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>()))
            .Returns(Task.CompletedTask);

        _mockHangfireEmailService.Setup(x => x.EnqueueEmail(It.IsAny<MailRequest>()))
            .Returns("job-id");

        // Act
        var result = await _authenticationService.VerifyEmail(request);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Result.Should().Be("Email sent successfully");

        _mockAuthenticationRepository.Verify(x => x.Register(It.Is<DomainUsers.User>(u =>
            u.Email == request.Email &&
            u.FullName == $"{request.FirstName} {request.LastName}" &&
            u.RoleId == userRole.RoleId &&
            u.AccountStatusId == accountStatus.StatusId)), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_WithExistingEmail_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        _mockAuthenticationRepository.Setup(x => x.IsEmailExists(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.VerifyEmail(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task VerifyEmail_WithMissingRequiredFields_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, "")
            .RuleFor(r => r.Password, "")
            .RuleFor(r => r.FirstName, "")
            .RuleFor(r => r.LastName, "")
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        // Act
        var result = await _authenticationService.VerifyEmail(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task VerifyOtp_WithValidOtp_ShouldSucceed()
    {
        // Arrange
        var otp = "123456";
        var email = "test@example.com";

        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, otp)
            .Generate();

        var otpData = new RegisterVerifyOtpResDto { Email = email, Otp = otp };

        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, email)
            .RuleFor(u => u.AccountStatusId, Guid.NewGuid())
            .Generate();

        var activeStatus = new Faker<DomainUsers.AccountStatus>()
            .RuleFor(s => s.StatusId, Guid.NewGuid())
            .RuleFor(s => s.Name, AccountStatusEnum.Active.ToString())
            .Generate();

        _mockRedisCacheService.Setup(x => x.Get<RegisterVerifyOtpResDto>(otp))
            .ReturnsAsync(otpData);

        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(email))
            .ReturnsAsync(user);

        _mockTypeRepository.Setup(x => x.GetAccountStatusById(AccountStatusEnum.Active))
            .ReturnsAsync(activeStatus);

        _mockAuthenticationRepository.Setup(x => x.UpdateUser(It.IsAny<DomainUsers.User>()))
            .ReturnsAsync(true);

        _mockRedisCacheService.Setup(x => x.Remove(otp))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authenticationService.VerifyOtp(request);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Result.Should().Be("Email verified successfully");

        _mockAuthenticationRepository.Verify(x => x.UpdateUser(It.Is<DomainUsers.User>(u =>
            u.AccountStatusId == activeStatus.StatusId)), Times.Once);
    }

    [Fact]
    public async Task VerifyOtp_WithInvalidOtp_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, "")
            .Generate();

        // Act
        var result = await _authenticationService.VerifyOtp(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task VerifyOtp_WithNonExistentOtp_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, "123456")
            .Generate();

        _mockRedisCacheService.Setup(x => x.Get<RegisterVerifyOtpResDto>(request.Otp))
            .ReturnsAsync((RegisterVerifyOtpResDto?)null);

        // Act
        var result = await _authenticationService.VerifyOtp(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task VerifyOtp_WithOtpMismatch_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, "123456")
            .Generate();

        var otpData = new RegisterVerifyOtpResDto { Email = "test@example.com", Otp = "654321" };

        _mockRedisCacheService.Setup(x => x.Get<RegisterVerifyOtpResDto>(request.Otp))
            .ReturnsAsync(otpData);

        // Act
        var result = await _authenticationService.VerifyOtp(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ResetPasswordVerify_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .Generate();

        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, request.Email)
            .Generate();

        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(request.Email))
            .ReturnsAsync(user);

        _mockRedisCacheService.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        _mockMailService.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>()))
            .Returns(Task.CompletedTask);

        _mockHangfireEmailService.Setup(x => x.EnqueueEmail(It.IsAny<MailRequest>()))
            .Returns("job-id");

        // Act
        var result = await _authenticationService.ResetPasswordVerify(request);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Result.Should().Be("OTP sent successfully");
    }

    [Fact]
    public async Task ResetPasswordVerify_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, "")
            .Generate();

        // Act
        var result = await _authenticationService.ResetPasswordVerify(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ResetPasswordVerify_WithNonExistentEmail_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .Generate();

        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(request.Email))
            .ReturnsAsync((DomainUsers.User?)null);

        // Act
        var result = await _authenticationService.ResetPasswordVerify(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var otp = "123456";
        var email = "test@example.com";
        var newPassword = "newPassword123";

        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, otp)
            .RuleFor(r => r.NewPassword, newPassword)
            .RuleFor(r => r.ConfirmPassword, newPassword)
            .Generate();

        var otpData = new RegisterVerifyOtpResDto { Email = email, Otp = otp };

        var user = new Faker<DomainUsers.User>()
            .RuleFor(u => u.UserId, Guid.NewGuid())
            .RuleFor(u => u.Email, email)
            .RuleFor(u => u.PasswordHash, "old_hash")
            .Generate();

        _mockRedisCacheService.Setup(x => x.Get<RegisterVerifyOtpResDto>(otp))
            .ReturnsAsync(otpData);

        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(email))
            .ReturnsAsync(user);

        _mockJwtService.Setup(x => x.HashObject<string>(newPassword))
            .Returns("new_hash");

        _mockAuthenticationRepository.Setup(x => x.UpdateUser(It.IsAny<DomainUsers.User>()))
            .ReturnsAsync(true);

        _mockRedisCacheService.Setup(x => x.Remove(otp))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authenticationService.ResetPassword(request);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Result.Should().Be("Password reset successfully");

        _mockAuthenticationRepository.Verify(x => x.UpdateUser(It.Is<DomainUsers.User>(u =>
            u.PasswordHash == "new_hash")), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithPasswordMismatch_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, "123456")
            .RuleFor(r => r.NewPassword, "password123")
            .RuleFor(r => r.ConfirmPassword, "password456")
            .Generate();

        // Act
        var result = await _authenticationService.ResetPassword(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ResetPassword_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, "123456")
            .RuleFor(r => r.NewPassword, "")
            .RuleFor(r => r.ConfirmPassword, "")
            .Generate();

        // Act
        var result = await _authenticationService.ResetPassword(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task ResetPassword_WithInvalidOtp_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, "123456")
            .RuleFor(r => r.NewPassword, "password123")
            .RuleFor(r => r.ConfirmPassword, "password123")
            .Generate();

        _mockRedisCacheService.Setup(x => x.Get<RegisterVerifyOtpResDto>(request.Otp))
            .ReturnsAsync((RegisterVerifyOtpResDto?)null);

        // Act
        var result = await _authenticationService.ResetPassword(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }
}

// Helper classes for testing
public class AccountStatus
{
    public Guid StatusId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserRole
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
}
