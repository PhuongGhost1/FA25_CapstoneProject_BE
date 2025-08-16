using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.AccessTool;

public class UserAccessToolEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IUserAccessToolService> _mockUserAccessToolService;
    private readonly Faker _faker;

    public UserAccessToolEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockUserAccessToolService = new Mock<IUserAccessToolService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        });
    }

    [Fact]
    public async Task GetAllUserAccessTools_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var userAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .RuleFor(uat => uat.GrantedAt, f => f.Date.Past())
            .RuleFor(uat => uat.ExpiredAt, f => f.Date.Future())
            .Generate(3);

        _mockUserAccessToolService.Setup(x => x.GetUserAccessToolsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<IReadOnlyList<UserAccessTool>, Error>(userAccessTools));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        // Add authorization header with user ID
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.GetAsync("/user-access-tool/get-all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserAccessTool>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(userAccessTools);
    }

    [Fact]
    public async Task GetAllUserAccessTools_WithNoAccessTools_ShouldReturnEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var emptyAccessTools = new List<UserAccessTool>();

        _mockUserAccessToolService.Setup(x => x.GetUserAccessToolsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<IReadOnlyList<UserAccessTool>, Error>(emptyAccessTools));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.GetAsync("/user-access-tool/get-all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserAccessTool>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllUserAccessTools_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var error = new Error("UserAccessTool.GetFailed", "Failed to get user access tools", ErrorType.Failure);

        _mockUserAccessToolService.Setup(x => x.GetUserAccessToolsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<IReadOnlyList<UserAccessTool>, Error>(error));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.GetAsync("/user-access-tool/get-all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllUserAccessTools_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/user-access-tool/get-all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveUserAccessTools_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var activeAccessTools = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, f => f.Random.Int(1, 10))
            .RuleFor(uat => uat.GrantedAt, f => f.Date.Past())
            .RuleFor(uat => uat.ExpiredAt, f => f.Date.Future())
            .Generate(2);

        _mockUserAccessToolService.Setup(x => x.GetActiveUserAccessToolsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<IReadOnlyList<UserAccessTool>, Error>(activeAccessTools));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.GetAsync("/user-access-tool/get-active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserAccessTool>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(activeAccessTools);
    }

    [Fact]
    public async Task GrantAccessToTool_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolId = 3;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var request = new GrantAccessToToolRequest
        {
            AccessToolId = accessToolId,
            ExpiredAt = expiredAt
        };

        var grantedAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, accessToolId)
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, expiredAt)
            .Generate();

        _mockUserAccessToolService.Setup(x => x.GrantAccessToToolAsync(userId, accessToolId, expiredAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<UserAccessTool, Error>(grantedAccessTool));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/grant-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<UserAccessTool>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(grantedAccessTool);
    }

    [Fact]
    public async Task GrantAccessToTool_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolId = 999;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var request = new GrantAccessToToolRequest
        {
            AccessToolId = accessToolId,
            ExpiredAt = expiredAt
        };

        var error = new Error("UserAccessTool.AccessToolNotFound", "Access tool not found", ErrorType.NotFound);

        _mockUserAccessToolService.Setup(x => x.GrantAccessToToolAsync(userId, accessToolId, expiredAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<UserAccessTool, Error>(error));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/grant-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GrantAccessToTool_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new GrantAccessToToolRequest
        {
            AccessToolId = 3,
            ExpiredAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await client.PostAsJsonAsync("/user-access-tool/grant-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeAccessToTool_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        var request = new RevokeAccessToToolRequest
        {
            AccessToolId = accessToolId
        };

        _mockUserAccessToolService.Setup(x => x.RevokeAccessToToolAsync(userId, accessToolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/revoke-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GrantMultipleAccessToTools_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolIds = new List<int> { 1, 2, 3 };
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var request = new GrantMultipleAccessToToolRequest
        {
            AccessToolIds = accessToolIds,
            ExpiredAt = expiredAt
        };

        _mockUserAccessToolService.Setup(x => x.GrantAccessToToolsAsync(userId, accessToolIds, expiredAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/grant-multiple-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAllAccessTools_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();

        _mockUserAccessToolService.Setup(x => x.RevokeAllAccessToolsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsync("/user-access-tool/revoke-all-access", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccessToolsForMembership_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var request = new UpdateAccessToolsForMembershipRequest
        {
            PlanId = planId,
            MembershipExpiryDate = membershipExpiryDate
        };

        _mockUserAccessToolService.Setup(x => x.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/update-access-tools-for-membership", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccessToolsForMembership_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var planId = 999;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var request = new UpdateAccessToolsForMembershipRequest
        {
            PlanId = planId,
            MembershipExpiryDate = membershipExpiryDate
        };

        var error = new Error("UserAccessTool.PlanNotFound", "Membership plan not found", ErrorType.NotFound);

        _mockUserAccessToolService.Setup(x => x.UpdateAccessToolsForMembershipAsync(userId, planId, membershipExpiryDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<bool, Error>(error));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/update-access-tools-for-membership", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllUserAccessTools_WithInvalidRoute_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/user-access-tool/invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GrantAccessToTool_WithPostMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/user-access-tool/get-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GrantAccessToTool_WithPutMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsync("/user-access-tool/get-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GrantAccessToTool_WithDeleteMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/user-access-tool/get-all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GrantAccessToTool_WithNullRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync<GrantAccessToToolRequest>("/user-access-tool/grant-access", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GrantAccessToTool_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidJson = "{ invalid json }";

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await authenticatedClient.PostAsync("/user-access-tool/grant-access", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GrantAccessToTool_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolId = 3;
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var request = new GrantAccessToToolRequest
        {
            AccessToolId = accessToolId,
            ExpiredAt = expiredAt
        };

        var grantedAccessTool = new Faker<UserAccessTool>()
            .RuleFor(uat => uat.UserAccessToolId, f => f.Random.Int(1, 100))
            .RuleFor(uat => uat.UserId, userId)
            .RuleFor(uat => uat.AccessToolId, accessToolId)
            .RuleFor(uat => uat.GrantedAt, DateTime.UtcNow)
            .RuleFor(uat => uat.ExpiredAt, expiredAt)
            .Generate();

        _mockUserAccessToolService.Setup(x => x.GrantAccessToToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<UserAccessTool, Error>(grantedAccessTool));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/grant-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockUserAccessToolService.Verify(x => x.GrantAccessToToolAsync(It.IsAny<Guid>(), accessToolId, expiredAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAccessToTool_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolId = 3;

        var request = new RevokeAccessToToolRequest
        {
            AccessToolId = accessToolId
        };

        _mockUserAccessToolService.Setup(x => x.RevokeAccessToToolAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/revoke-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockUserAccessToolService.Verify(x => x.RevokeAccessToToolAsync(It.IsAny<Guid>(), accessToolId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantMultipleAccessToTools_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var accessToolIds = new List<int> { 1, 2, 3 };
        var expiredAt = DateTime.UtcNow.AddDays(30);

        var request = new GrantMultipleAccessToToolRequest
        {
            AccessToolIds = accessToolIds,
            ExpiredAt = expiredAt
        };

        _mockUserAccessToolService.Setup(x => x.GrantAccessToToolsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/grant-multiple-access", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockUserAccessToolService.Verify(x => x.GrantAccessToToolsAsync(It.IsAny<Guid>(), accessToolIds, expiredAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccessToolsForMembership_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var planId = 3;
        var membershipExpiryDate = DateTime.UtcNow.AddDays(365);

        var request = new UpdateAccessToolsForMembershipRequest
        {
            PlanId = planId,
            MembershipExpiryDate = membershipExpiryDate
        };

        _mockUserAccessToolService.Setup(x => x.UpdateAccessToolsForMembershipAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Create authenticated client
        var authenticatedClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockUserAccessToolService.Object);
            });
        }).CreateClient();

        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid_token");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/user-access-tool/update-access-tools-for-membership", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockUserAccessToolService.Verify(x => x.UpdateAccessToolsForMembershipAsync(It.IsAny<Guid>(), planId, membershipExpiryDate, It.IsAny<CancellationToken>()), Times.Once);
    }
}

// Request DTOs for testing
public class GrantAccessToToolRequest
{
    public int AccessToolId { get; set; }
    public DateTime ExpiredAt { get; set; }
}

public class RevokeAccessToToolRequest
{
    public int AccessToolId { get; set; }
}

public class GrantMultipleAccessToToolRequest
{
    public IEnumerable<int> AccessToolIds { get; set; } = new List<int>();
    public DateTime ExpiredAt { get; set; }
}

public class UpdateAccessToolsForMembershipRequest
{
    public int PlanId { get; set; }
    public DateTime MembershipExpiryDate { get; set; }
}
