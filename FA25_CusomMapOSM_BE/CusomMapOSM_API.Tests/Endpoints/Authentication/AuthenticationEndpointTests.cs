using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Response;
using CusomMapOSM_API.Endpoints.Authentication;
using CusomMapOSM_Domain.Entities.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.Authentication;

public class AuthenticationEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly Faker _faker;

    public AuthenticationEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockAuthenticationService.Object);
            });
        });
    }

    [Fact]
    public async Task Login_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        var response = new Faker<LoginResDto>()
            .RuleFor(r => r.Token, f => f.Random.AlphaNumeric(100))
            .Generate();

        _mockAuthenticationService.Setup(x => x.Login(request))
            .ReturnsAsync(Option.Some<LoginResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<LoginResDto>();
        result.Should().NotBeNull();
        result!.Token.Should().Be(response.Token);
    }

    [Fact]
    public async Task Login_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, "") // Invalid: empty email
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        var error = new Error("Authentication.InvalidEmailOrPassword", "Invalid email or password", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.Login(request))
            .ReturnsAsync(Option.None<LoginResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Authentication.InvalidEmailOrPassword");
    }

    [Fact]
    public async Task Login_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        _mockAuthenticationService.Setup(x => x.Login(request))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task VerifyEmail_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Email sent successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.VerifyEmail(request))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/verify-email", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<RegisterResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Email sent successfully");
    }

    [Fact]
    public async Task VerifyEmail_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        var error = new Error("Authentication.EmailAlreadyExists", "Email already exists", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.VerifyEmail(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Authentication.EmailAlreadyExists");
    }

    [Fact]
    public async Task VerifyOtp_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Email verified successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.VerifyOtp(request))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/verify-otp", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<RegisterResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Email verified successfully");
    }

    [Fact]
    public async Task VerifyOtp_WithInvalidOtp_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, "invalid_otp")
            .Generate();

        var error = new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.VerifyOtp(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-otp", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Authentication.InvalidOtp");
    }

    [Fact]
    public async Task Logout_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();

        // Create a client with authentication
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockAuthenticationService.Object);
            });
        }).CreateClient();

        // Add authorization header with user ID
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Logout successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.LogOut(userId))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await authenticatedClient.PostAsync("/auth/logout", null);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<RegisterResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Logout successfully");
    }

    [Fact]
    public async Task Logout_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();

        var error = new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound);

        _mockAuthenticationService.Setup(x => x.LogOut(userId))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPasswordVerify_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "OTP sent successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.ResetPasswordVerify(request))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/reset-password-verify", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<RegisterResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("OTP sent successfully");
    }

    [Fact]
    public async Task ResetPasswordVerify_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .Generate();

        var error = new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound);

        _mockAuthenticationService.Setup(x => x.ResetPasswordVerify(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/reset-password-verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Authentication.UserNotFound");
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .RuleFor(r => r.NewPassword, f => f.Internet.Password())
            .RuleFor(r => r.ConfirmPassword, (f, r) => r.NewPassword) // Same password
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Password reset successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.ResetPassword(request))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/reset-password", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await httpResponse.Content.ReadFromJsonAsync<RegisterResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Password reset successfully");
    }

    [Fact]
    public async Task ResetPassword_WithPasswordMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .RuleFor(r => r.NewPassword, "password123")
            .RuleFor(r => r.ConfirmPassword, "password456") // Different password
            .Generate();

        var error = new Error("Authentication.PasswordMismatch", "Password mismatch", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.ResetPassword(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Authentication.PasswordMismatch");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public async Task Login_WithInvalidJson_ShouldReturnBadRequest(string invalidJson)
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNullRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<LoginReqDto>("/auth/login", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { }; // Missing all required fields

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyEmail_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, "") // Invalid: empty email
            .RuleFor(r => r.Password, "") // Invalid: empty password
            .RuleFor(r => r.FirstName, "") // Invalid: empty first name
            .RuleFor(r => r.LastName, "") // Invalid: empty last name
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        var error = new Error("Authentication.InvalidEmailOrPassword", "Invalid email or password", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.VerifyEmail(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyOtp_WithEmptyOtp_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, "") // Invalid: empty OTP
            .Generate();

        var error = new Error("Authentication.InvalidOtp", "Invalid OTP", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.VerifyOtp(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-otp", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPasswordVerify_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, "") // Invalid: empty email
            .Generate();

        var error = new Error("Authentication.InvalidEmail", "Invalid email", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.ResetPasswordVerify(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/reset-password-verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithEmptyPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .RuleFor(r => r.NewPassword, "") // Invalid: empty password
            .RuleFor(r => r.ConfirmPassword, "") // Invalid: empty confirm password
            .Generate();

        var error = new Error("Authentication.InvalidPassword", "Invalid password", ErrorType.Validation);

        _mockAuthenticationService.Setup(x => x.ResetPassword(request))
            .ReturnsAsync(Option.None<RegisterResDto, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<LoginReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .Generate();

        var response = new Faker<LoginResDto>()
            .RuleFor(r => r.Token, f => f.Random.AlphaNumeric(100))
            .Generate();

        _mockAuthenticationService.Setup(x => x.Login(It.IsAny<LoginReqDto>()))
            .ReturnsAsync(Option.Some<LoginResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockAuthenticationService.Verify(x => x.Login(It.Is<LoginReqDto>(r =>
            r.Email == request.Email &&
            r.Password == request.Password)), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<RegisterVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password())
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber())
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Email sent successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.VerifyEmail(It.IsAny<RegisterVerifyReqDto>()))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/verify-email", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockAuthenticationService.Verify(x => x.VerifyEmail(It.Is<RegisterVerifyReqDto>(r =>
            r.Email == request.Email &&
            r.Password == request.Password &&
            r.FirstName == request.FirstName &&
            r.LastName == request.LastName &&
            r.Phone == request.Phone)), Times.Once);
    }

    [Fact]
    public async Task VerifyOtp_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<VerifyOtpReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Email verified successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.VerifyOtp(It.IsAny<VerifyOtpReqDto>()))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/verify-otp", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockAuthenticationService.Verify(x => x.VerifyOtp(It.Is<VerifyOtpReqDto>(r =>
            r.Otp == request.Otp)), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordVerify_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordVerifyReqDto>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "OTP sent successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.ResetPasswordVerify(It.IsAny<ResetPasswordVerifyReqDto>()))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/reset-password-verify", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockAuthenticationService.Verify(x => x.ResetPasswordVerify(It.Is<ResetPasswordVerifyReqDto>(r =>
            r.Email == request.Email)), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ResetPasswordReqDto>()
            .RuleFor(r => r.Otp, f => f.Random.Replace("######"))
            .RuleFor(r => r.NewPassword, f => f.Internet.Password())
            .RuleFor(r => r.ConfirmPassword, (f, r) => r.NewPassword)
            .Generate();

        var response = new Faker<RegisterResDto>()
            .RuleFor(r => r.Result, "Password reset successfully")
            .Generate();

        _mockAuthenticationService.Setup(x => x.ResetPassword(It.IsAny<ResetPasswordReqDto>()))
            .ReturnsAsync(Option.Some<RegisterResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/reset-password", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockAuthenticationService.Verify(x => x.ResetPassword(It.Is<ResetPasswordReqDto>(r =>
            r.Otp == request.Otp &&
            r.NewPassword == request.NewPassword &&
            r.ConfirmPassword == request.ConfirmPassword)), Times.Once);
    }
}

// Helper class for ProblemDetails deserialization
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
}
