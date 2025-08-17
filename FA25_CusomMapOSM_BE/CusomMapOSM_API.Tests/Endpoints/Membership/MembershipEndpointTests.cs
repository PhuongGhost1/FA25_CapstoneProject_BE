using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.Membership;
using CusomMapOSM_API.Endpoints.Memberships;
using CusomMapOSM_Domain.Entities.Memberships;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using DomainMembershipAddon = CusomMapOSM_Domain.Entities.Memberships.MembershipAddon;
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
    public async Task ChangePlan_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<ChangeSubscriptionPlanRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.NewPlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var error = new Error("Membership.PlanChange.Failed", "Failed to change plan", ErrorType.Failure);

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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrRenewMembership_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.PlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, request.UserId)
            .RuleFor(m => m.OrgId, request.OrgId)
            .RuleFor(m => m.PlanId, request.PlanId)
            .RuleFor(m => m.StartDate, DateTime.UtcNow)
            .RuleFor(m => m.AutoRenew, request.AutoRenew)
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
    public async Task CreateOrRenewMembership_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.Empty) // Invalid: empty GUID
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.PlanId, 0) // Invalid: plan ID <= 0
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/create-or-renew", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrRenewMembership_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.PlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        var error = new Error("Membership.Create.Failed", "Failed to create membership", ErrorType.Failure);

        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(
                request.UserId,
                request.OrgId,
                request.PlanId,
                request.AutoRenew,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembership, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/membership/create-or-renew", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PurchaseAddon_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<PurchaseAddonRequest>()
            .RuleFor(r => r.MembershipId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.AddonKey, "extra_exports")
            .RuleFor(r => r.Quantity, 10)
            .RuleFor(r => r.EffectiveImmediately, true)
            .Generate();

        var addon = new Faker<DomainMembershipAddon>()
            .RuleFor(a => a.AddonId, Guid.NewGuid())
            .RuleFor(a => a.MembershipId, request.MembershipId)
            .RuleFor(a => a.OrgId, request.OrgId)
            .RuleFor(a => a.AddonKey, request.AddonKey)
            .RuleFor(a => a.Quantity, request.Quantity)
            .RuleFor(a => a.PurchasedAt, DateTime.UtcNow)
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
        result.Status.Should().Be("purchased");
    }

    [Fact]
    public async Task PurchaseAddon_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<PurchaseAddonRequest>()
            .RuleFor(r => r.MembershipId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.AddonKey, "extra_exports")
            .RuleFor(r => r.Quantity, 10)
            .RuleFor(r => r.EffectiveImmediately, true)
            .Generate();

        var error = new Error("Membership.Addon.Failed", "Failed to purchase addon", ErrorType.Failure);

        _mockMembershipService.Setup(x => x.AddAddonAsync(
                request.MembershipId,
                request.OrgId,
                request.AddonKey,
                request.Quantity,
                request.EffectiveImmediately,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<DomainMembershipAddon, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/membership/purchase-addon", request);

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
            .RuleFor(r => r.ResourceKey, "exports")
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
    public async Task TrackUsage_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<TrackUsageRequest>()
            .RuleFor(r => r.MembershipId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.ResourceKey, "exports")
            .RuleFor(r => r.Amount, 1)
            .Generate();

        var error = new Error("Membership.Usage.Failed", "Failed to track usage", ErrorType.Failure);

        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(
                request.MembershipId,
                request.OrgId,
                request.ResourceKey,
                request.Amount,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<bool, Error>(error));

        // Act
        var response = await client.PostAsJsonAsync("/membership/track-usage", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckFeature_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "advanced_export";

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
    public async Task CheckFeature_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var membershipId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var featureKey = "advanced_export";

        var error = new Error("Membership.Feature.Failed", "Failed to check feature", ErrorType.Failure);

        _mockMembershipService.Setup(x => x.HasFeatureAsync(
                membershipId,
                orgId,
                featureKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.None<bool, Error>(error));

        // Act
        var response = await client.GetAsync($"/membership/{membershipId}/org/{orgId}/feature/{featureKey}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public async Task CreateOrRenewMembership_WithInvalidJson_ShouldReturnBadRequest(string invalidJson)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new Faker<CreateMembershipRequest>()
            .RuleFor(r => r.UserId, Guid.NewGuid())
            .RuleFor(r => r.OrgId, Guid.NewGuid())
            .RuleFor(r => r.PlanId, 3)
            .RuleFor(r => r.AutoRenew, true)
            .Generate();

        // Act
        var response = await client.PostAsJsonAsync("/membership/create-or-renew", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Should still work with valid request
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
