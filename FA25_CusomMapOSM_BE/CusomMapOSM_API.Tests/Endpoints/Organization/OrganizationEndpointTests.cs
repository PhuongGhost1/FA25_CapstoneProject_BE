using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Optional;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CusomMapOSM_API.Tests.Endpoints.Organization;

public class OrganizationEndpointTests : IClassFixture<WebApplicationFactory<CusomMapOSM_API.Program>>
{
    private readonly WebApplicationFactory<CusomMapOSM_API.Program> _factory;
    private readonly Mock<IOrganizationService> _mockOrganizationService;
    private readonly Faker _faker;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrganizationEndpointTests(WebApplicationFactory<CusomMapOSM_API.Program> factory)
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

    #region Create Organization Tests

    [Fact]
    public async Task CreateOrganization_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new Faker<OrganizationReqDto>()
            .RuleFor(r => r.OrgName, f => f.Company.CompanyName())
            .RuleFor(r => r.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
            .RuleFor(r => r.LogoUrl, f => f.Internet.Url())
            .RuleFor(r => r.ContactEmail, f => f.Internet.Email())
            .RuleFor(r => r.ContactPhone, f => f.Phone.PhoneNumber())
            .RuleFor(r => r.Address, f => f.Address.FullAddress())
            .Generate();

        var response = new OrganizationResDto { Result = "Organization created successfully" };

        _mockOrganizationService.Setup(x => x.Create(request))
            .ReturnsAsync(Option.Some<OrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<OrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Organization created successfully");
    }

    [Fact]
    public async Task CreateOrganization_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new OrganizationReqDto
        {
            OrgName = "", // Invalid: empty name
            Abbreviation = "ABC",
            Description = "Test Description"
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
    public async Task CreateOrganization_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No authentication
        var request = new OrganizationReqDto
        {
            OrgName = "Test Org",
            Abbreviation = "TO",
            Description = "Test"
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get All Organizations Tests

    [Fact]
    public async Task GetAllOrganizations_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var organizations = new Faker<OrganizationDetailDto>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.Description, f => f.Lorem.Sentence())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .RuleFor(o => o.ContactEmail, f => f.Internet.Email())
            .RuleFor(o => o.ContactPhone, f => f.Phone.PhoneNumber())
            .RuleFor(o => o.Address, f => f.Address.FullAddress())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.IsActive, f => f.Random.Bool())
            .Generate(3);

        var response = new GetAllOrganizationsResDto { Organizations = organizations };

        _mockOrganizationService.Setup(x => x.GetAll())
            .ReturnsAsync(Option.Some<GetAllOrganizationsResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync("/organizations");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetAllOrganizationsResDto>();
        result.Should().NotBeNull();
        result!.Organizations.Should().HaveCount(3);
    }

    #endregion

    #region Get Organization By ID Tests

    [Fact]
    public async Task GetOrganizationById_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var organization = new Faker<OrganizationDetailDto>()
            .RuleFor(o => o.OrgId, orgId)
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.Description, f => f.Lorem.Sentence())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.IsActive, true)
            .Generate();

        var response = new GetOrganizationByIdResDto { Organization = organization };

        _mockOrganizationService.Setup(x => x.GetById(orgId))
            .ReturnsAsync(Option.Some<GetOrganizationByIdResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync($"/organizations/{orgId}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetOrganizationByIdResDto>();
        result.Should().NotBeNull();
        result!.Organization.OrgId.Should().Be(orgId);
    }

    [Fact]
    public async Task GetOrganizationById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var error = new Error("Organization.NotFound", "Organization not found", ErrorType.NotFound);

        _mockOrganizationService.Setup(x => x.GetById(orgId))
            .ReturnsAsync(Option.None<GetOrganizationByIdResDto, Error>(error));

        // Act
        var httpResponse = await client.GetAsync($"/organizations/{orgId}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Organization Tests

    [Fact]
    public async Task UpdateOrganization_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var request = new Faker<OrganizationReqDto>()
            .RuleFor(r => r.OrgName, f => f.Company.CompanyName())
            .RuleFor(r => r.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
            .Generate();

        var response = new UpdateOrganizationResDto { Result = "Organization updated successfully" };

        _mockOrganizationService.Setup(x => x.Update(orgId, request))
            .ReturnsAsync(Option.Some<UpdateOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PutAsJsonAsync($"/organizations/{orgId}", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<UpdateOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Organization updated successfully");
    }

    #endregion

    #region Delete Organization Tests

    [Fact]
    public async Task DeleteOrganization_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var orgId = Guid.NewGuid();
        var response = new DeleteOrganizationResDto { Result = "Organization deleted successfully" };

        _mockOrganizationService.Setup(x => x.Delete(orgId))
            .ReturnsAsync(Option.Some<DeleteOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.DeleteAsync($"/organizations/{orgId}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<DeleteOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Organization deleted successfully");
    }

    #endregion

    #region Invite Member Tests

    [Fact]
    public async Task InviteMember_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new Faker<InviteMemberOrganizationReqDto>()
            .RuleFor(r => r.OrgId, f => f.Random.Guid())
            .RuleFor(r => r.MemberEmail, f => f.Internet.Email())
            .RuleFor(r => r.MemberType, f => f.PickRandom("Admin", "Member", "Viewer"))
            .Generate();

        var response = new InviteMemberOrganizationResDto { Result = "Invitation sent successfully" };

        _mockOrganizationService.Setup(x => x.InviteMember(request))
            .ReturnsAsync(Option.Some<InviteMemberOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/invite-member", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<InviteMemberOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Invitation sent successfully");
    }

    [Fact]
    public async Task InviteMember_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberEmail = "invalid-email", // Invalid email format
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

    #endregion

    #region Accept Invite Tests

    [Fact]
    public async Task AcceptInvite_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new AcceptInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var response = new AcceptInviteOrganizationResDto { Result = "Invitation accepted successfully" };

        _mockOrganizationService.Setup(x => x.AcceptInvite(request))
            .ReturnsAsync(Option.Some<AcceptInviteOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/accept-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<AcceptInviteOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Invitation accepted successfully");
    }

    #endregion

    #region Get My Invitations Tests

    [Fact]
    public async Task GetMyInvitations_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var invitations = new Faker<InvitationDto>()
            .RuleFor(i => i.InvitationId, f => f.Random.Guid())
            .RuleFor(i => i.OrgId, f => f.Random.Guid())
            .RuleFor(i => i.OrgName, f => f.Company.CompanyName())
            .RuleFor(i => i.Email, f => f.Internet.Email())
            .RuleFor(i => i.InviterEmail, f => f.Internet.Email())
            .RuleFor(i => i.MemberType, f => f.PickRandom("Admin", "Member", "Viewer"))
            .RuleFor(i => i.InvitedAt, f => f.Date.Recent())
            .RuleFor(i => i.IsAccepted, false)
            .RuleFor(i => i.AcceptedAt, (DateTime?)null)
            .Generate(2);

        var response = new GetInvitationsResDto { Invitations = invitations };

        _mockOrganizationService.Setup(x => x.GetMyInvitations())
            .ReturnsAsync(Option.Some<GetInvitationsResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync("/organizations/my-invitations");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetInvitationsResDto>();
        result.Should().NotBeNull();
        result!.Invitations.Should().HaveCount(2);
    }

    #endregion

    #region Get Organization Members Tests

    [Fact]
    public async Task GetOrganizationMembers_WithValidOrgId_ShouldReturnSuccess()
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
            .Generate(3);

        var response = new GetOrganizationMembersResDto { Members = members };

        _mockOrganizationService.Setup(x => x.GetMembers(orgId))
            .ReturnsAsync(Option.Some<GetOrganizationMembersResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync($"/organizations/{orgId}/members");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetOrganizationMembersResDto>();
        result.Should().NotBeNull();
        result!.Members.Should().HaveCount(3);
    }

    #endregion

    #region Update Member Role Tests

    [Fact]
    public async Task UpdateMemberRole_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new UpdateMemberRoleReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            NewRole = "Admin"
        };

        var response = new UpdateMemberRoleResDto { Result = "Member role updated successfully" };

        _mockOrganizationService.Setup(x => x.UpdateMemberRole(request))
            .ReturnsAsync(Option.Some<UpdateMemberRoleResDto, Error>(response));

        // Act
        var httpResponse = await client.PutAsJsonAsync("/organizations/members/role", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<UpdateMemberRoleResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Member role updated successfully");
    }

    #endregion

    #region Remove Member Tests

    [Fact]
    public async Task RemoveMember_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new RemoveMemberReqDto
        {
            OrgId = Guid.NewGuid(),
            MemberId = Guid.NewGuid()
        };

        var response = new RemoveMemberResDto { Result = "Member removed successfully" };

        _mockOrganizationService.Setup(x => x.RemoveMember(request))
            .ReturnsAsync(Option.Some<RemoveMemberResDto, Error>(response));

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
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<RemoveMemberResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Member removed successfully");
    }

    #endregion

    #region Get My Organizations Tests

    [Fact]
    public async Task GetMyOrganizations_ShouldReturnSuccess()
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
            .Generate(2);

        var response = new GetMyOrganizationsResDto { Organizations = organizations };

        _mockOrganizationService.Setup(x => x.GetMyOrganizations())
            .ReturnsAsync(Option.Some<GetMyOrganizationsResDto, Error>(response));

        // Act
        var httpResponse = await client.GetAsync("/organizations/my-organizations");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<GetMyOrganizationsResDto>();
        result.Should().NotBeNull();
        result!.Organizations.Should().HaveCount(2);
    }

    #endregion

    #region Transfer Ownership Tests

    [Fact]
    public async Task TransferOwnership_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new TransferOwnershipReqDto
        {
            OrgId = Guid.NewGuid(),
            NewOwnerId = Guid.NewGuid()
        };

        var response = new TransferOwnershipResDto { Result = "Ownership transferred successfully" };

        _mockOrganizationService.Setup(x => x.TransferOwnership(request))
            .ReturnsAsync(Option.Some<TransferOwnershipResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/transfer-ownership", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<TransferOwnershipResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Ownership transferred successfully");
    }

    #endregion

    #region Reject Invite Tests

    [Fact]
    public async Task RejectInvite_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new RejectInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var response = new RejectInviteOrganizationResDto { Result = "Invitation rejected successfully" };

        _mockOrganizationService.Setup(x => x.RejectInvite(request))
            .ReturnsAsync(Option.Some<RejectInviteOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/reject-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<RejectInviteOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Invitation rejected successfully");
    }

    #endregion

    #region Cancel Invite Tests

    [Fact]
    public async Task CancelInvite_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CancelInviteOrganizationReqDto
        {
            InvitationId = Guid.NewGuid()
        };

        var response = new CancelInviteOrganizationResDto { Result = "Invitation cancelled successfully" };

        _mockOrganizationService.Setup(x => x.CancelInvite(request))
            .ReturnsAsync(Option.Some<CancelInviteOrganizationResDto, Error>(response));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/organizations/cancel-invite", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await httpResponse.Content.ReadFromJsonAsync<CancelInviteOrganizationResDto>();
        result.Should().NotBeNull();
        result!.Result.Should().Be("Invitation cancelled successfully");
    }

    #endregion
}