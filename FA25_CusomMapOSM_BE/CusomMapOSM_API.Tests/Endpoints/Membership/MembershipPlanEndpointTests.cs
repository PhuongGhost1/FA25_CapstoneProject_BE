using Bogus;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_API.Endpoints.Memberships;
using CusomMapOSM_Domain.Entities.Memberships;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.Membership;

public class MembershipPlanEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IMembershipPlanService> _mockMembershipPlanService;
    private readonly Faker _faker;

    public MembershipPlanEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockMembershipPlanService = new Mock<IMembershipPlanService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockMembershipPlanService.Object);
            });
        });
    }

    [Fact]
    public async Task GetActivePlans_WithActivePlans_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
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

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<Plan>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(activePlans);
    }

    [Fact]
    public async Task GetActivePlans_WithNoActivePlans_ShouldReturnEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var emptyPlans = new List<Plan>();

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPlans);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<Plan>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivePlans_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetPlanById_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
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

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(plan);
        result!.PlanId.Should().Be(planId);
        result.PlanName.Should().Be("Pro");
    }

    [Fact]
    public async Task GetPlanById_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = 999;

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanById_WithZeroId_ShouldReturnNull()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = 0;

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanById_WithNegativeId_ShouldReturnNull()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = -1;

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanById_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = 3;

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetPlanById_WithVariousIds_ShouldCallServiceWithCorrectId(int planId)
    {
        // Arrange
        var client = _factory.CreateClient();
        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, f => f.PickRandom("Basic", "Pro", "Enterprise"))
            .RuleFor(p => p.IsActive, true)
            .Generate();

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().NotBeNull();
        result!.PlanId.Should().Be(planId);
        
        _mockMembershipPlanService.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlans_WithLargeNumberOfPlans_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
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

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<Plan>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        result.Should().BeEquivalentTo(activePlans);
    }

    [Fact]
    public async Task GetPlanById_WithInactivePlan_ShouldReturnPlan()
    {
        // Arrange
        var client = _factory.CreateClient();
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

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactivePlan);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(inactivePlan);
        result!.PlanId.Should().Be(planId);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetPlanById_WithUnlimitedPlan_ShouldReturnPlan()
    {
        // Arrange
        var client = _factory.CreateClient();
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

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unlimitedPlan);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(unlimitedPlan);
        result!.PlanId.Should().Be(planId);
        result.MaxMapsPerMonth.Should().Be(-1);
        result.ExportQuota.Should().Be(-1);
        result.MaxUsersPerOrg.Should().Be(-1);
    }

    [Fact]
    public async Task GetPlanById_WithMaxIntId_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = int.MaxValue;

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Plan>();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActivePlans_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var activePlans = new Faker<Plan>()
            .RuleFor(p => p.PlanId, f => f.Random.Int(1, 10))
            .RuleFor(p => p.PlanName, f => f.PickRandom("Basic", "Pro", "Enterprise"))
            .RuleFor(p => p.IsActive, true)
            .Generate(3);

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockMembershipPlanService.Verify(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanById_WithServiceCall_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var planId = 3;
        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, planId)
            .RuleFor(p => p.PlanName, "Pro")
            .RuleFor(p => p.IsActive, true)
            .Generate();

        _mockMembershipPlanService.Setup(x => x.GetPlanByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var response = await client.GetAsync($"/membership-plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _mockMembershipPlanService.Verify(x => x.GetPlanByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePlans_WithInvalidRoute_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/membership-plan/invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlanById_WithInvalidRoute_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/membership-plan/invalid/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetActivePlans_WithPostMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/membership-plan/active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetPlanById_WithPostMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/membership-plan/3", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetActivePlans_WithPutMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsync("/membership-plan/active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetPlanById_WithPutMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsync("/membership-plan/3", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetActivePlans_WithDeleteMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetPlanById_WithDeleteMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/membership-plan/3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetActivePlans_WithNullReturnFromService_ShouldReturnEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Plan>?)null);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<Plan>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivePlans_WithLargeResponse_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
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
            .Generate(1000);

        _mockMembershipPlanService.Setup(x => x.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var response = await client.GetAsync("/membership-plan/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<Plan>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1000);
    }
}
