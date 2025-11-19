using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using CusomMapOSM_Domain.Entities.Memberships;
using DomainOrganization = CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspace;
using CusomMapOSM_Infrastructure.Features.Organization;
using CusomMapOSM_Infrastructure.Services;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;

namespace CusomMapOSM_Infrastructure.Tests.Features.Organization;

public class OrganizationServiceTests
{
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<IAuthenticationRepository> _mockAuthenticationRepository;
    private readonly Mock<ITypeRepository> _mockTypeRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<HangfireEmailService> _mockHangfireEmailService;
    private readonly Mock<IMembershipService> _mockMembershipService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IWorkspaceRepository> _mockWorkspaceRepository;
    private readonly OrganizationService _organizationService;
    private readonly Faker _faker;

    static OrganizationServiceTests()
    {
        // Initialize Hangfire with in-memory storage for tests (only once, before any test runs)
        var storage = new MemoryStorage();
        GlobalConfiguration.Configuration.UseStorage(storage);
        JobStorage.Current = storage;
    }

    public OrganizationServiceTests()
    {
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockAuthenticationRepository = new Mock<IAuthenticationRepository>();
        _mockTypeRepository = new Mock<ITypeRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockHangfireEmailService = new Mock<HangfireEmailService>();
        _mockMembershipService = new Mock<IMembershipService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockWorkspaceRepository = new Mock<IWorkspaceRepository>();

        _organizationService = new OrganizationService(
            _mockOrganizationRepository.Object,
            _mockAuthenticationRepository.Object,
            _mockTypeRepository.Object,
            _mockCurrentUserService.Object,
            _mockHangfireEmailService.Object,
            _mockMembershipService.Object,
            _mockJwtService.Object,
            _mockWorkspaceRepository.Object
        );
        _faker = new Faker();
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new OrganizationReqDto
        {
            OrgName = "Test Organization",
            Abbreviation = "TO",
            Description = "Test Description",
            LogoUrl = "logo.png",
            ContactEmail = "contact@test.com",
            ContactPhone = "1234567890",
            Address = "123 Test St"
        };

        var createdOrg = new DomainOrganization.Organization
        {
            OrgId = orgId,
            OrgName = request.OrgName,
            OwnerUserId = userId
        };

        var user = new DomainUser.User
        {
            UserId = userId,
            FullName = "Test User"
        };

        var membership = new DomainMembership
        {
            MembershipId = Guid.NewGuid(),
            UserId = userId,
            OrgId = orgId
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.CreateOrganization(It.IsAny<DomainOrganization.Organization>()))
            .ReturnsAsync(true);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(createdOrg);
        _mockOrganizationRepository.Setup(x => x.AddMemberToOrganization(It.IsAny<DomainOrganization.OrganizationMember>()))
            .ReturnsAsync(true);
        _mockMembershipService.Setup(x => x.CreateOrRenewMembershipAsync(userId, orgId, 1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockAuthenticationRepository.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(user);
        _mockWorkspaceRepository.Setup(x => x.CreateAsync(It.IsAny<CusomMapOSM_Domain.Entities.Workspaces.Workspace>()))
            .ReturnsAsync(new CusomMapOSM_Domain.Entities.Workspaces.Workspace { WorkspaceId = Guid.NewGuid(), Creator = user });

        // Act
        var result = await _organizationService.Create(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("Success");
        response.OrgId.Should().Be(orgId);
    }

    [Fact]
    public async Task Create_WithCreateFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new OrganizationReqDto
        {
            OrgName = "Test Organization",
            Abbreviation = "TO"
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.CreateOrganization(It.IsAny<DomainOrganization.Organization>()))
            .ReturnsAsync(false);

        // Act
        var result = await _organizationService.Create(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithOrganizations_ShouldReturnAll()
    {
        // Arrange
        var organizations = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.AlphaNumeric(5))
            .RuleFor(o => o.IsActive, true)
            .RuleFor(o => o.CreatedAt, DateTime.UtcNow)
            .Generate(3);

        _mockOrganizationRepository.Setup(x => x.GetAllOrganizations())
            .ReturnsAsync(organizations);

        // Act
        var result = await _organizationService.GetAll();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Organizations.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoOrganizations_ShouldReturnEmptyList()
    {
        // Arrange
        _mockOrganizationRepository.Setup(x => x.GetAllOrganizations())
            .ReturnsAsync(new List<DomainOrganization.Organization>());

        // Act
        var result = await _organizationService.GetAll();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Organizations.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOrganization()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, "Test Organization")
            .RuleFor(o => o.IsActive, true)
            .Generate();

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);

        // Act
        var result = await _organizationService.GetById(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Organization.OrgId.Should().Be(orgId);
        response.Organization.OrgName.Should().Be("Test Organization");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync((DomainOrganization.Organization?)null);

        // Act
        var result = await _organizationService.GetById(orgId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, "Old Name")
            .Generate();

        var request = new OrganizationReqDto
        {
            OrgName = "New Name",
            Abbreviation = "NN",
            Description = "New Description"
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.UpdateOrganization(It.IsAny<DomainOrganization.Organization>()))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.Update(orgId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        organization.OrgName.Should().Be("New Name");
    }

    [Fact]
    public async Task Update_WithUnauthenticatedUser_ShouldReturnError()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var request = new OrganizationReqDto
        {
            OrgName = "New Name",
            Abbreviation = "NN"
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns((Guid?)null);

        // Act
        var result = await _organizationService.Update(orgId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Unauthorized)
        );
    }

    [Fact]
    public async Task Update_WithNonExistentOrganization_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new OrganizationReqDto
        {
            OrgName = "New Name",
            Abbreviation = "NN"
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync((DomainOrganization.Organization?)null);

        // Act
        var result = await _organizationService.Update(orgId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.DeleteOrganization(orgId))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.Delete(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
    }

    [Fact]
    public async Task Delete_WithUnauthenticatedUser_ShouldReturnError()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns((Guid?)null);

        // Act
        var result = await _organizationService.Delete(orgId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Unauthorized)
        );
    }

    #endregion

    #region InviteMember Tests

    [Fact]
    public async Task InviteMember_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = orgId,
            MemberEmail = "newmember@test.com",
            MemberType = "Viewer"
        };

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, userId)
            .RuleFor(o => o.OrgName, "Test Org")
            .Generate();

        var ownerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, 1)
            .RuleFor(p => p.MaxUsersPerOrg, 10)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.OrgId, orgId)
            .RuleFor(m => m.Plan, plan)
            .Generate();

        var members = new List<DomainOrganization.OrganizationMember> { ownerMember };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationByEmailAndOrg(request.MemberEmail, orgId))
            .ReturnsAsync((DomainOrganization.OrganizationInvitation?)null);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(ownerMember);
        _mockMembershipService.Setup(x => x.GetMembershipByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMembers(orgId))
            .ReturnsAsync(members);
        _mockOrganizationRepository.Setup(x => x.InviteMemberToOrganization(It.IsAny<DomainOrganization.OrganizationInvitation>()))
            .ReturnsAsync(true);
        _mockAuthenticationRepository.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(new DomainUser.User { UserId = userId, FullName = "Test User" });

        // Act
        var result = await _organizationService.InviteMember(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
    }

    [Fact]
    public async Task InviteMember_WithExistingPendingInvitation_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = orgId,
            MemberEmail = "existing@test.com",
            MemberType = "Viewer"
        };

        var existingInvitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.Email, f => f.Internet.Email())
            .RuleFor(i => i.OrgId, orgId)
            .RuleFor(i => i.Status, DomainOrganization.Enums.InvitationStatus.Pending)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationByEmailAndOrg(request.MemberEmail, orgId))
            .ReturnsAsync(existingInvitation);

        // Act
        var result = await _organizationService.InviteMember(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Conflict)
        );
    }

    [Fact]
    public async Task InviteMember_WithQuotaExceeded_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new InviteMemberOrganizationReqDto
        {
            OrgId = orgId,
            MemberEmail = "newmember@test.com",
            MemberType = "Viewer"
        };

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, userId)
            .Generate();

        var ownerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        var plan = new Faker<Plan>()
            .RuleFor(p => p.PlanId, 1)
            .RuleFor(p => p.MaxUsersPerOrg, 5)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.Plan, plan)
            .Generate();

        // Create 5 active members (at quota limit)
        var members = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Status, MemberStatus.Active)
            .Generate(5);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationByEmailAndOrg(request.MemberEmail, orgId))
            .ReturnsAsync((DomainOrganization.OrganizationInvitation?)null);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(ownerMember);
        _mockMembershipService.Setup(x => x.GetMembershipByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMembers(orgId))
            .ReturnsAsync(members);

        // Act
        var result = await _organizationService.InviteMember(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Conflict)
        );
    }

    #endregion

    #region AcceptInvite Tests

    [Fact]
    public async Task AcceptInvite_WithValidInvitation_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var request = new AcceptInviteOrganizationReqDto
        {
            InvitationId = invitationId
        };

        var invitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, invitationId)
            .RuleFor(i => i.OrgId, f => f.Random.Guid())
            .RuleFor(i => i.Email, f => f.Internet.Email())
            .RuleFor(i => i.Status, DomainOrganization.Enums.InvitationStatus.Pending)
            .RuleFor(i => i.ExpiresAt, DateTime.UtcNow.AddDays(7))
            .RuleFor(i => i.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Viewer)
            .Generate();

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, Guid.NewGuid())
            .Generate();

        var ownerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .Generate();

        var plan = new Faker<Plan>()
            .RuleFor(p => p.MaxUsersPerOrg, 10)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.Plan, plan)
            .Generate();

        var user = new Faker<DomainUser.User>()
            .RuleFor(u => u.UserId, f => f.Random.Guid())
            .RuleFor(u => u.Email, invitation.Email)
            .Generate();

        var members = new List<DomainOrganization.OrganizationMember> { ownerMember };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockCurrentUserService.Setup(x => x.GetEmail()).Returns(invitation.Email);
        _mockOrganizationRepository.Setup(x => x.GetInvitationById(invitationId))
            .ReturnsAsync(invitation);
        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(invitation.Email))
            .ReturnsAsync(user);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(organization.OwnerUserId, orgId))
            .ReturnsAsync(ownerMember);
        _mockMembershipService.Setup(x => x.GetMembershipByUserOrgAsync(organization.OwnerUserId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(organization.OwnerUserId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMembers(orgId))
            .ReturnsAsync(members);
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(It.IsAny<Guid>(), orgId, "users", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));
        _mockOrganizationRepository.Setup(x => x.AddMemberToOrganization(It.IsAny<DomainOrganization.OrganizationMember>()))
            .ReturnsAsync(true);
        _mockOrganizationRepository.Setup(x => x.UpdateInvitation(It.IsAny<DomainOrganization.OrganizationInvitation>()))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.AcceptInvite(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public async Task AcceptInvite_WithExpiredInvitation_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var request = new AcceptInviteOrganizationReqDto
        {
            InvitationId = invitationId
        };

        var invitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, invitationId)
            .RuleFor(i => i.Status, DomainOrganization.Enums.InvitationStatus.Expired)
            .RuleFor(i => i.ExpiresAt, DateTime.UtcNow.AddDays(-1))
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationById(invitationId))
            .ReturnsAsync(invitation);

        // Act
        var result = await _organizationService.AcceptInvite(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Conflict)
        );
    }

    #endregion

    #region GetMyInvitations Tests

    [Fact]
    public async Task GetMyInvitations_WithValidEmail_ShouldReturnInvitations()
    {
        // Arrange
        var email = "user@test.com";
        var invitations = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, f => f.Random.Guid())
            .RuleFor(i => i.Email, email)
            .RuleFor(i => i.Status, DomainOrganization.Enums.InvitationStatus.Pending)
            .RuleFor(i => i.InvitedAt, DateTime.UtcNow)
            .RuleFor(i => i.Organization, new DomainOrganization.Organization { OrgId = Guid.NewGuid(), OrgName = "Test Org" })
            .RuleFor(i => i.Inviter, new DomainUser.User { Email = "inviter@test.com" })
            .Generate(3);

        _mockCurrentUserService.Setup(x => x.GetEmail()).Returns(email);
        _mockOrganizationRepository.Setup(x => x.GetInvitationsByEmail(email))
            .ReturnsAsync(invitations);

        // Act
        var result = await _organizationService.GetMyInvitations();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Invitations.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMyInvitations_WithNoEmail_ShouldReturnError()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.GetEmail()).Returns((string?)null);

        // Act
        var result = await _organizationService.GetMyInvitations();

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Unauthorized)
        );
    }

    #endregion

    #region GetMembers Tests

    [Fact]
    public async Task GetMembers_WithValidOrgId_ShouldReturnMembers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .Generate();

        var members = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.MemberId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Viewer)
            .RuleFor(m => m.Status, MemberStatus.Active)
            .RuleFor(m => m.JoinedAt, DateTime.UtcNow)
            .RuleFor(m => m.User, new DomainUser.User { Email = "member@test.com", FullName = "Member User" })
            .Generate(3);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMembers(orgId))
            .ReturnsAsync(members);

        // Act
        var result = await _organizationService.GetMembers(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Members.Should().HaveCount(3);
    }

    #endregion

    #region UpdateMemberRole Tests

    [Fact]
    public async Task UpdateMemberRole_WithOwnerUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var request = new UpdateMemberRoleReqDto
        {
            OrgId = orgId,
            MemberId = memberId,
            NewRole = "Admin"
        };

        var currentUserMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        var member = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.MemberId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Viewer)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(currentUserMember);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberById(memberId))
            .ReturnsAsync(member);
        _mockOrganizationRepository.Setup(x => x.UpdateOrganizationMember(It.IsAny<DomainOrganization.OrganizationMember>()))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.UpdateMemberRole(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        member.Role.Should().Be(OrganizationMemberTypeEnum.Admin);
    }

    [Fact]
    public async Task UpdateMemberRole_WithNonOwnerUser_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new UpdateMemberRoleReqDto
        {
            OrgId = orgId,
            MemberId = Guid.NewGuid(),
            NewRole = "Admin"
        };

        var currentUserMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Viewer) // Not Owner
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(currentUserMember);

        // Act
        var result = await _organizationService.UpdateMemberRole(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Forbidden)
        );
    }

    #endregion

    #region RemoveMember Tests

    [Fact]
    public async Task RemoveMember_WithOwnerUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var request = new RemoveMemberReqDto
        {
            OrgId = orgId,
            MemberId = memberId
        };

        var currentUserMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        var member = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.MemberId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .Generate();

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, f => f.Random.Guid())
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(currentUserMember);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberById(memberId))
            .ReturnsAsync(member);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockMembershipService.Setup(x => x.GetMembershipByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(It.IsAny<Guid>(), orgId, "users", -1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));
        _mockOrganizationRepository.Setup(x => x.RemoveOrganizationMember(memberId))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.RemoveMember(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
    }

    #endregion

    #region RejectInvite Tests

    [Fact]
    public async Task RejectInvite_WithValidInvitation_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var request = new RejectInviteOrganizationReqDto
        {
            InvitationId = invitationId
        };

        var invitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, invitationId)
            .RuleFor(i => i.Email, "user@test.com")
            .RuleFor(i => i.Status, InvitationStatus.Pending)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockCurrentUserService.Setup(x => x.GetEmail()).Returns(invitation.Email);
        _mockOrganizationRepository.Setup(x => x.GetInvitationById(invitationId))
            .ReturnsAsync(invitation);
        _mockOrganizationRepository.Setup(x => x.DeleteInvitation(invitationId))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.RejectInvite(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
    }

    #endregion

    #region CancelInvite Tests

    [Fact]
    public async Task CancelInvite_WithValidInvitation_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var request = new CancelInviteOrganizationReqDto
        {
            InvitationId = invitationId
        };

        var invitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, invitationId)
            .RuleFor(i => i.InvitedBy, userId)
            .RuleFor(i => i.Status, InvitationStatus.Pending)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationById(invitationId))
            .ReturnsAsync(invitation);
        _mockOrganizationRepository.Setup(x => x.DeleteInvitation(invitationId))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.CancelInvite(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
    }

    [Fact]
    public async Task CancelInvite_WithNotInviter_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var request = new CancelInviteOrganizationReqDto
        {
            InvitationId = invitationId
        };

        var invitation = new Faker<DomainOrganization.OrganizationInvitation>()
            .RuleFor(i => i.InvitationId, invitationId)
            .RuleFor(i => i.InvitedBy, otherUserId) // Different user
            .RuleFor(i => i.Status, InvitationStatus.Pending)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetInvitationById(invitationId))
            .ReturnsAsync(invitation);

        // Act
        var result = await _organizationService.CancelInvite(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Forbidden)
        );
    }

    #endregion

    #region GetMyOrganizations Tests

    [Fact]
    public async Task GetMyOrganizations_WithValidUser_ShouldReturnOrganizations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var members = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Viewer)
            .RuleFor(m => m.JoinedAt, DateTime.UtcNow)
            .RuleFor(m => m.Organization, new DomainOrganization.Organization
            {
                OrgId = Guid.NewGuid(),
                OrgName = "Test Org",
                Abbreviation = "TO",
                LogoUrl = "logo.png"
            })
            .Generate(2);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetUserOrganizations(userId))
            .ReturnsAsync(members);

        // Act
        var result = await _organizationService.GetMyOrganizations();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Organizations.Should().HaveCount(2);
    }

    #endregion

    #region TransferOwnership Tests

    [Fact]
    public async Task TransferOwnership_WithValidData_ShouldSucceed()
    {
        // Arrange
        var currentOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new TransferOwnershipReqDto
        {
            NewOwnerId = newOwnerId
        };

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, currentOwnerId)
            .Generate();

        var newOwnerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Admin)
            .Generate();

        var currentOwnerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(currentOwnerId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(newOwnerId, orgId))
            .ReturnsAsync(newOwnerMember);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(currentOwnerId, orgId))
            .ReturnsAsync(currentOwnerMember);
        _mockOrganizationRepository.Setup(x => x.UpdateOrganizationMember(It.IsAny<DomainOrganization.OrganizationMember>()))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.TransferOwnership(orgId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        newOwnerMember.Role.Should().Be(OrganizationMemberTypeEnum.Owner);
        currentOwnerMember.Role.Should().Be(OrganizationMemberTypeEnum.Admin);
    }

    [Fact]
    public async Task TransferOwnership_WithNonMemberNewOwner_ShouldReturnError()
    {
        // Arrange
        var currentOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new TransferOwnershipReqDto
        {
            NewOwnerId = newOwnerId
        };

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, currentOwnerId)
            .Generate();

        var currentOwnerMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(currentOwnerId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(newOwnerId, orgId))
            .ReturnsAsync((DomainOrganization.OrganizationMember?)null);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(currentOwnerId, orgId))
            .ReturnsAsync(currentOwnerMember);

        // Act
        var result = await _organizationService.TransferOwnership(orgId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region BulkCreateStudents Tests

    [Fact]
    public async Task BulkCreateStudents_WithValidExcelFile_ShouldCreateStudents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new BulkCreateStudentsRequest
        {
            OrganizationId = orgId,
            Domain = "school.edu"
        };

        // Create a mock Excel file
        var excelBytes = CreateMockExcelFile(new[] { ("John Doe", "Class A"), ("Jane Smith", "Class B") });
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("students.xlsx");
        mockFile.Setup(f => f.Length).Returns(excelBytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(excelBytes));

        var organization = new Faker<DomainOrganization.Organization>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OwnerUserId, userId)
            .Generate();

        var currentUserMember = new Faker<DomainOrganization.OrganizationMember>()
            .RuleFor(m => m.UserId, f => f.Random.Guid())
            .RuleFor(m => m.OrgId, f => f.Random.Guid())
            .RuleFor(m => m.Role, DomainOrganization.Enums.OrganizationMemberTypeEnum.Owner)
            .Generate();

        var plan = new Faker<Plan>()
            .RuleFor(p => p.MaxUsersPerOrg, 10)
            .Generate();

        var membership = new Faker<DomainMembership>()
            .RuleFor(m => m.MembershipId, Guid.NewGuid())
            .RuleFor(m => m.Plan, plan)
            .Generate();

        var members = new List<DomainOrganization.OrganizationMember> { currentUserMember };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId))
            .ReturnsAsync(organization);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMemberByUserAndOrg(userId, orgId))
            .ReturnsAsync(currentUserMember);
        _mockMembershipService.Setup(x => x.GetMembershipByUserOrgAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockMembershipService.Setup(x => x.GetCurrentMembershipWithIncludesAsync(userId, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<DomainMembership, Error>(membership));
        _mockOrganizationRepository.Setup(x => x.GetOrganizationMembers(orgId))
            .ReturnsAsync(members);
        _mockAuthenticationRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>()))
            .ReturnsAsync((DomainUser.User?)null); // No existing users
        _mockJwtService.Setup(x => x.HashObject<string>(It.IsAny<string>()))
            .Returns("hashed_password");
        _mockAuthenticationRepository.Setup(x => x.Register(It.IsAny<DomainUser.User>()))
            .ReturnsAsync(true);
        _mockOrganizationRepository.Setup(x => x.AddMemberToOrganization(It.IsAny<DomainOrganization.OrganizationMember>()))
            .ReturnsAsync(true);
        _mockMembershipService.Setup(x => x.TryConsumeQuotaAsync(It.IsAny<Guid>(), orgId, "users", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _organizationService.BulkCreateStudents(mockFile.Object, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.TotalCreated.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BulkCreateStudents_WithEmptyFile_ShouldReturnError()
    {
        // Arrange
        var request = new BulkCreateStudentsRequest
        {
            OrganizationId = Guid.NewGuid(),
            Domain = "school.edu"
        };

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _organizationService.BulkCreateStudents(mockFile.Object, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task BulkCreateStudents_WithFileTooLarge_ShouldReturnError()
    {
        // Arrange
        var request = new BulkCreateStudentsRequest
        {
            OrganizationId = Guid.NewGuid(),
            Domain = "school.edu"
        };

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("large.xlsx");
        mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11MB

        // Act
        var result = await _organizationService.BulkCreateStudents(mockFile.Object, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task BulkCreateStudents_WithInvalidFileFormat_ShouldReturnError()
    {
        // Arrange
        var request = new BulkCreateStudentsRequest
        {
            OrganizationId = Guid.NewGuid(),
            Domain = "school.edu"
        };

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("students.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);

        // Act
        var result = await _organizationService.BulkCreateStudents(mockFile.Object, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    private byte[] CreateMockExcelFile(IEnumerable<(string Name, string Class)> students)
    {
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Students");
        
        // Headers
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Class";
        
        // Data
        int row = 2;
        foreach (var (name, studentClass) in students)
        {
            worksheet.Cells[row, 1].Value = name;
            worksheet.Cells[row, 2].Value = studentClass;
            row++;
        }
        
        return package.GetAsByteArray();
    }

    #endregion
}

