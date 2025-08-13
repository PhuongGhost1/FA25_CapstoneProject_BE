using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.Membership;
using CusomMapOSM_API.Endpoints.Memberships;
using CusomMapOSM_Domain.Entities.Memberships;
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

namespace CusomMapOSM_API.Tests.Endpoints.Membership;

public class MembershipEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<IMembershipPlanService> _mockMembershipPlanService;
    private readonly Faker _faker;

    public MembershipEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockMembershipService = new Mock<IMembershipService>();
        _mockMembershipPlanService = new Mock<IMembershipPlanService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockMembershipService.Object);
                services.AddScoped(_ => _mockMembershipPlanService.Object);
            });
        });
    }

    [Fact]
    public async Task ChangePlan_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, request.UserId)
            .RuleFor(m => m.OrgId, request.OrgId)
            .RuleFor(m => m.PlanId, request.NewPlanId)
            .RuleFor(m => m.StartDate, DateTime.UtcNow)
            .RuleFor(m => m.AutoRenew, request.AutoRenew)
            .Generate();

        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(
                request.UserId,
                request.OrgId,
                request.NewPlanId,
                request.AutoRenew,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ChangeSubscriptionPlanResponse>();
        result.Should().NotBeNull();
        result!.MembershipId.Should().Be(membership.MembershipId);
        result.Status.Should().Be("Plan changed successfully");
        result.EffectiveDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ChangePlan_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 0) // Invalid: plan ID <= 0
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePlan_WithServiceError_ShouldReturnProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 999) // Non-existent plan
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var error = new Error("Membership.PlanNotFound", "Plan not found", ErrorType.NotFound);

        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(
                request.UserId,
                request.OrgId,
                request.NewPlanId,
                request.AutoRenew,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Membership.PlanNotFound");
    }

    [Fact]
    public async Task CreateOrRenew_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.PlanId, 2)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, request.UserId)
            .RuleFor(m => m.OrgId, request.OrgId)
            .RuleFor(m => m.PlanId, request.PlanId)
            .Generate();

        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(
                request.UserId,
                request.OrgId,
                request.PlanId,
                request.AutoRenew,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));

        // Act
        var response = await client.PostAsJsonAsync("/membership/create-or-renew", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreateMembershipResponse>();
        result.Should().NotBeNull();
        result!.MembershipId.Should().Be(membership.MembershipId);
    }

    [Fact]
    public async Task CreateOrRenew_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.OrgId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.PlanId, 0) // Invalid: plan ID <= 0
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/create-or-renew", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TrackUsage_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<TrackUsageRequest>()
            .RuleFor(r => r.MembershipId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.ResourceKey, "maps")
            .RuleFor(r => r.Amount, 1)
            .Generate();

        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(
                request.MembershipId,
                request.OrgId,
                request.ResourceKey,
                request.Amount,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var response = await client.PostAsJsonAsync("/membership/track-usage", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TrackUsageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TrackUsage_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<TrackUsageRequest>()
            .RuleFor(r => r.MembershipId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.OrgId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.ResourceKey, "maps")
            .RuleFor(r => r.Amount, 0) // Invalid: amount <= 0
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/track-usage", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HasFeature_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "analytics";

        _mockMembershipService.Setup(x => x.HasFeatureAsync(
                membershipId,
                orgId,
                featureKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var response = await client.GetAsync($"/membership/{membershipId}/org/{orgId}/feature/{featureKey}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<FeatureCheckResponse>();
        result.Should().NotBeNull();
        result!.HasFeature.Should().BeTrue();
    }

    [Fact]
    public async Task PurchaseAddon_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<PurchaseAddonRequest>()
            .RuleFor(r => r.MembershipId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.AddonKey, "extra_maps")
            .RuleFor(r => r.Quantity, 10)
            .RuleFor(r => r.EffectiveImmediately, true)
            .Generate();

        var addon = new Faker<DomainMembershipAddon>()
            .RuleFor(a => a.AddonId, Guid.NewGuid())
            .RuleFor(a => a.MembershipId, request.MembershipId)
            .RuleFor(a => a.OrgId, request.OrgId)
            .RuleFor(a => a.AddonKey, request.AddonKey)
            .RuleFor(a => a.Quantity, request.Quantity)
            .Generate();

        _mockMembershipService.Setup(x => x.AddAddonAsync(
                request.MembershipId,
                request.OrgId,
                request.AddonKey,
                request.Quantity,
                request.EffectiveImmediately,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembershipAddon, Error>(addon));

        // Act
        var response = await client.PostAsJsonAsync("/membership/purchase-addon", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PurchaseAddonResponse>();
        result.Should().NotBeNull();
        result!.AddonId.Should().Be(addon.AddonId);
        result.Status.Should().Be("created");
    }

    [Fact]
    public async Task PurchaseAddon_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<PurchaseAddonRequest>()
            .RuleFor(r => r.MembershipId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.OrgId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.AddonKey, "") // Invalid: empty string
            .RuleFor(r => r.Quantity, 10)
            .RuleFor(r => r.EffectiveImmediately, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/purchase-addon", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task ChangePlan_WithInvalidJson_ShouldReturnBadRequest(string invalidJson)
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/membership/change-plan", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePlan_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ChangePlan_WithLargePlanId_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, int.MaxValue) // Very large plan ID
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var error = new Error("Membership.PlanNotFound", "Plan not found", ErrorType.NotFound);

        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(
                request.UserId,
                request.OrgId,
                request.NewPlanId,
                request.AutoRenew,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePlan_WithNegativePlanId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, -1) // Negative plan ID
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePlan_WithNullRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<ChangeSubscriptionPlanRequest>("/membership/change-plan", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePlan_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { }; // Missing all required fields

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePlan_WithValidRequest_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 3)
            .RuleFor(r => r.AutoRenew, false)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, request.UserId)
            .RuleFor(m => m.OrgId, request.OrgId)
            .RuleFor(m => m.PlanId, request.NewPlanId)
            .Generate();

        _mockMembershipService.Setup(x => x.ChangeSubscriptionPlanAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));

        // Act
        var response = await client.PostAsJsonAsync("/membership/change-plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockMembershipService.Verify(x => x.ChangeSubscriptionPlanAsync(
            request.UserId,
            request.OrgId,
            request.NewPlanId,
            request.AutoRenew,
            It.IsAny<CancellationToken>()), Times.Once);
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
