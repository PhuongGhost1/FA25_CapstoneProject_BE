using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.Organization;

public class OrganizationEndpointIntegrationTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IOrganizationService> _mockOrganizationService;
    private readonly Faker _faker;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrganizationEndpointIntegrationTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
    {
        _mockOrganizationService = new Mock<IOrganizationService>();
        _faker = new Faker();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockOrganizationService.Object);
            });
        });
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User-Id", _testUserId.ToString());
        return client;
    }

    #region Edge Case Tests

    [Fact]
    public async Task CreateOrganization_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new OrganizationReqDto
        {
            OrgName = "Existing Organization",
            Abbreviation = "EO",
            Description = "Test"
        };

        var error = new Error("Organization.DuplicateName", "Organization name already exists", ErrorType.Conflict);

        _mockOrganizationService.Setup(x => x.Create(request))
            .ReturnsAsync(Option.None<OrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrganization_WithLongDescription_ShouldHandleCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var longDescription = new string('A', 2000); // Very long description
        var request = new OrganizationReqDto
        {
            OrgName = "Test Organization",
            Abbreviation = "TO",
            Description = longDescription
        };

        var response = new OrganizationResDto 
        { 
            Result = "Organization created successfully",
            OrgId = Guid.NewGuid()
        };

        _mockOrganizationService.Setup(x => x.Create(request))
            .ReturnsAsync(Option.Some<OrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InviteMember_WithSameEmailTwice_ShouldReturnConflict()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberEmail = "duplicate@test.com",
            MemberType = "Member"
        };

        var error = new Error("Organization.DuplicateInvitation", "User already invited", ErrorType.Conflict);

        _mockOrganizationService.Setup(x => x.InviteMember(request))
            .ReturnsAsync(Option.None<InviteMemberOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/invite-member", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AcceptInvite_WithExpiredInvitation_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new AcceptInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var error = new Error("Organization.ExpiredInvitation", "Invitation has expired", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.AcceptInvite(request))
            .ReturnsAsync(Option.None<AcceptInviteOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/accept-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Permission Tests

    [Fact]
    public async Task UpdateOrganization_WithoutOwnerPermission_ShouldReturnForbidden()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var request = new OrganizationReqDto
        {
            OrgName = "Updated Name",
            Abbreviation = "UN",
            Description = "Updated Description"
        };

        var error = new Error("Organization.InsufficientPermissions", "Insufficient permissions", ErrorType.Forbidden);

        _mockOrganizationService.Setup(x => x.Update(orgId, request))
            .ReturnsAsync(Option.None<UpdateOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PutAsJsonAsync($"/organizations/{orgId}", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteOrganization_WithActiveMembers_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();

        var error = new Error("Organization.HasActiveMembers", "Cannot delete organization with active members", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.Delete(orgId))
            .ReturnsAsync(Option.None<DeleteOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.DeleteAsync($"/organizations/{orgId}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InviteMember_WithoutInvitePermission_ShouldReturnForbidden()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberEmail = "newmember@test.com",
            MemberType = "Member"
        };

        var error = new Error("Organization.CannotInvite", "No permission to invite members", ErrorType.Forbidden);

        _mockOrganizationService.Setup(x => x.InviteMember(request))
            .ReturnsAsync(Option.None<InviteMemberOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/invite-member", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task GetMyOrganizations_WithLargeNumberOfOrgs_ShouldReturnPaginated()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var organizations = new Faker<MyOrganizationDto>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.MyRole, f => f.PickRandom("Owner", "Admin", "Member"))
            .RuleFor(o => o.JoinedAt, f => f.Date.Past())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .Generate(50); // Large number of organizations

        var response = new GetMyOrganizationsResDto { Organizations = organizations };

        _mockOrganizationService.Setup(x => x.GetMyOrganizations())
            .ReturnsAsync(Option.Some<GetMyOrganizationsResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync("/organizations/my-organizations");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetMyOrganizationsResDto>();
        result.Should().NotBeNull();
        result!.Organizations.Should().HaveCount(50);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithLargeNumberOfMembers_ShouldReturnAll()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var members = new Faker<MemberDto>()
            .RuleFor(m => m.MemberId, f => f.Random.Guid())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Role, f => f.PickRandom("Owner", "Admin", "Member", "Viewer"))
            .RuleFor(m => m.JoinedAt, f => f.Date.Past())
            .RuleFor(m => m.IsActive, true)
            .Generate(100); // Large number of members

        var response = new GetOrganizationMembersResDto { Members = members };

        _mockOrganizationService.Setup(x => x.GetMembers(orgId))
            .ReturnsAsync(Option.Some<GetOrganizationMembersResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync($"/organizations/{orgId}/members");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetOrganizationMembersResDto>();
        result.Should().NotBeNull();
        result!.Members.Should().HaveCount(100);
    }

    #endregion

    #region Data Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrganization_WithInvalidOrgName_ShouldReturnBadRequest(string invalidName)
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new OrganizationReqDto
        {
            OrgName = invalidName,
            Abbreviation = "ABC",
            Description = "Test"
        };

        var error = new Error("Organization.InvalidName", "Organization name is required", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.Create(request))
            .ReturnsAsync(Option.None<OrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrganization_WithNullOrgName_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new OrganizationReqDto
        {
            OrgName = null!,
            Abbreviation = "ABC",
            Description = "Test"
        };

        var error = new Error("Organization.InvalidName", "Organization name is required", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.Create(request))
            .ReturnsAsync(Option.None<OrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    [InlineData("test@")]
    [InlineData("")]
    public async Task InviteMember_WithInvalidEmailFormats_ShouldReturnBadRequest(string invalidEmail)
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberEmail = invalidEmail,
            MemberType = "Member"
        };

        var error = new Error("Organization.InvalidEmail", "Invalid email format", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.InviteMember(request))
            .ReturnsAsync(Option.None<InviteMemberOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/invite-member", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("")]
    [InlineData("ADMIN")] // Case sensitivity test
    public async Task InviteMember_WithInvalidRole_ShouldReturnBadRequest(string invalidRole)
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberEmail = "test@example.com",
            MemberType = invalidRole
        };

        var error = new Error("Organization.InvalidRole", "Invalid role specified", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.InviteMember(request))
            .ReturnsAsync(Option.None<InviteMemberOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/invite-member", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetOrganizationById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var nonExistentId = Guid.NewGuid();

        var error = new Error("Organization.NotFound", "Organization not found", ErrorType.NotFound);

        _mockOrganizationService.Setup(x => x.GetById(nonExistentId))
            .ReturnsAsync(Option.None<GetOrganizationByIdResDto, Error>(error));

        // Act
        var httpResponse = await client.GetAsync($"/organizations/{nonExistentId}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TransferOwnership_ToSameOwner_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var request = new TransferOwnershipReqDto
        {
            NewOwnerId = _testUserId // Same as current user
        };

        var error = new Error("Organization.SameOwner", "Cannot transfer ownership to current owner", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.TransferOwnership(orgId, request))
            .ReturnsAsync(Option.None<TransferOwnershipResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync($"/api/v1/organizations/{orgId}/ownership", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveMember_RemovingLastAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new RemoveMemberReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberId = Guid.NewGuid()
        };

        var error = new Error("Organization.CannotRemoveLastAdmin", "Cannot remove last admin", ErrorType.Validation);

        _mockOrganizationService.Setup(x => x.RemoveMember(request))
            .ReturnsAsync(Option.None<RemoveMemberResDto, Error>(error));

        // Act
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/organizations/members/remove")
        {
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var httpResponse = await client.SendAsync(httpRequest);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task AcceptInvite_AlreadyAccepted_ShouldReturnConflict()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new AcceptInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var error = new Error("Organization.InvitationAlreadyAccepted", "Invitation already accepted", ErrorType.Conflict);

        _mockOrganizationService.Setup(x => x.AcceptInvite(request))
            .ReturnsAsync(Option.None<AcceptInviteOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/accept-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelInvite_AlreadyCancelled_ShouldReturnConflict()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CancelInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var error = new Error("Organization.InvitationAlreadyCancelled", "Invitation already cancelled", ErrorType.Conflict);

        _mockOrganizationService.Setup(x => x.CancelInvite(request))
            .ReturnsAsync(Option.None<CancelInviteOrganizationResDto, Error>(error));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/cancel-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion
}