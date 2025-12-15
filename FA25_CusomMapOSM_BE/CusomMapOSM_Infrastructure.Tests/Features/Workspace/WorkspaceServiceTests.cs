using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using DomainOrganization = CusomMapOSM_Domain.Entities.Organizations;
using DomainUser = CusomMapOSM_Domain.Entities.Users;
using DomainWorkspace = CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Workspaces.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Features.Workspaces;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Workspace;

public class WorkspaceServiceTests
{
    private readonly Mock<IWorkspaceRepository> _mockWorkspaceRepository;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<IMapRepository> _mockMapRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IQuestionBankRepository> _mockQuestionBankRepository;
    private readonly WorkspaceService _workspaceService;
    private readonly Faker _faker;

    public WorkspaceServiceTests()
    {
        _mockWorkspaceRepository = new Mock<IWorkspaceRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockMapRepository = new Mock<IMapRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockQuestionBankRepository = new Mock<IQuestionBankRepository>();
        _workspaceService = new WorkspaceService(
            _mockWorkspaceRepository.Object,
            _mockOrganizationRepository.Object,
            _mockMapRepository.Object,
            _mockQuestionBankRepository.Object,
            _mockCurrentUserService.Object
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
        var request = new WorkspaceReqDto
        {
            OrgId = orgId,
            WorkspaceName = "Test Workspace",
            Description = "Test Description",
            Icon = "icon.png",
            Access = WorkspaceAccessEnum.AllMembers
        };

        var organization = new DomainOrganization.Organization { OrgId = orgId, OrgName = "Test Org" };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync(organization);
        _mockWorkspaceRepository.Setup(x => x.CreateAsync(It.IsAny<DomainWorkspace.Workspace>())).ReturnsAsync(new DomainWorkspace.Workspace { WorkspaceId = Guid.NewGuid(), Creator = new DomainUser.User { FullName = "Test User" } });
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Map>());

        // Act
        var result = await _workspaceService.Create(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        _mockWorkspaceRepository.Verify(x => x.CreateAsync(It.Is<DomainWorkspace.Workspace>(w =>
            w.OrgId == orgId &&
            w.CreatedBy == userId &&
            w.WorkspaceName == request.WorkspaceName)), Times.Once);
    }

    [Fact]
    public async Task Create_WithNonExistentOrganization_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var request = new WorkspaceReqDto
        {
            OrgId = orgId,
            WorkspaceName = "Test Workspace"
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockOrganizationRepository.Setup(x => x.GetOrganizationById(orgId)).ReturnsAsync((DomainOrganization.Organization?)null);

        // Act
        var result = await _workspaceService.Create(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithWorkspaces_ShouldReturnAll()
    {
        // Arrange
        var workspaces = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.WorkspaceName, f => f.Company.CompanyName())
            .RuleFor(w => w.Description, f => f.Lorem.Sentence())
            .RuleFor(w => w.Access, WorkspaceAccessEnum.AllMembers)
            .RuleFor(w => w.IsActive, true)
            .RuleFor(w => w.CreatedAt, DateTime.UtcNow)
            .RuleFor(w => w.Organization, (DomainOrganization.Organization?)null)
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate(3);

        _mockWorkspaceRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(workspaces);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Map>());

        // Act
        var result = await _workspaceService.GetAll();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspaces.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoWorkspaces_ShouldReturnEmptyList()
    {
        // Arrange
        _mockWorkspaceRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<DomainWorkspace.Workspace>());

        // Act
        var result = await _workspaceService.GetAll();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspaces.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnWorkspace()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, workspaceId)
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .RuleFor(w => w.Description, "Test Description")
            .RuleFor(w => w.Access, WorkspaceAccessEnum.AllMembers)
            .RuleFor(w => w.IsActive, true)
            .RuleFor(w => w.CreatedAt, DateTime.UtcNow)
            .RuleFor(w => w.Organization, new DomainOrganization.Organization { OrgName = "Test Org" })
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(workspaceId)).ReturnsAsync(new List<Map>());

        // Act
        var result = await _workspaceService.GetById(workspaceId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspace.WorkspaceId.Should().Be(workspaceId);
        response.Workspace.WorkspaceName.Should().Be("Test Workspace");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync((DomainWorkspace.Workspace?)null);

        // Act
        var result = await _workspaceService.GetById(workspaceId);

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
        var workspaceId = Guid.NewGuid();
        var existingWorkspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, workspaceId)
            .RuleFor(w => w.WorkspaceName, "Old Name")
            .RuleFor(w => w.Description, "Old Description")
            .RuleFor(w => w.Access, WorkspaceAccessEnum.AllMembers)
            .Generate();

        var request = new UpdateWorkspaceReqDto
        {
            WorkspaceName = "New Name",
            Description = "New Description",
            Icon = "new-icon.png",
            Access = WorkspaceAccessEnum.AllMembers
        };

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(existingWorkspace);
        _mockWorkspaceRepository.Setup(x => x.UpdateAsync(It.IsAny<DomainWorkspace.Workspace>())).ReturnsAsync(new DomainWorkspace.Workspace { WorkspaceId = workspaceId, WorkspaceName = "New Name", Description = "New Description", Access = WorkspaceAccessEnum.AllMembers, Creator = new DomainUser.User { FullName = "Test User" } });

        // Act
        var result = await _workspaceService.Update(workspaceId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        existingWorkspace.WorkspaceName.Should().Be("New Name");
        existingWorkspace.Description.Should().Be("New Description");
        existingWorkspace.Access.Should().Be(WorkspaceAccessEnum.AllMembers);
        _mockWorkspaceRepository.Verify(x => x.UpdateAsync(existingWorkspace), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistentWorkspace_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var request = new UpdateWorkspaceReqDto
        {
            WorkspaceName = "New Name"
        };

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync((DomainWorkspace.Workspace?)null);

        // Act
        var result = await _workspaceService.Update(workspaceId, request);

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
        var workspaceId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.ExistsAsync(workspaceId)).ReturnsAsync(true);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(workspaceId)).ReturnsAsync(new List<Map>());
        _mockQuestionBankRepository.Setup(x => x.GetQuestionBanksByWorkspaceId(workspaceId)).ReturnsAsync(new List<QuestionBank>());
        _mockWorkspaceRepository.Setup(x => x.DeleteAsync(workspaceId)).ReturnsAsync(true);

        // Act
        var result = await _workspaceService.Delete(workspaceId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        _mockWorkspaceRepository.Verify(x => x.DeleteAsync(workspaceId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.ExistsAsync(workspaceId)).ReturnsAsync(false);

        // Act
        var result = await _workspaceService.Delete(workspaceId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task Delete_WithActiveMaps_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var activeMap = new Faker<Map>()
            .RuleFor(m => m.MapId, Guid.NewGuid())
            .RuleFor(m => m.IsActive, true)
            .Generate();

        _mockWorkspaceRepository.Setup(x => x.ExistsAsync(workspaceId)).ReturnsAsync(true);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(workspaceId)).ReturnsAsync(new List<Map> { activeMap });

        // Act
        var result = await _workspaceService.Delete(workspaceId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Validation);
                error.Code.Should().Be("Workspace.HasActiveMaps");
            }
        );
    }

    [Fact]
    public async Task Delete_WithActiveQuestionBanks_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var activeQuestionBank = new Faker<QuestionBank>()
            .RuleFor(qb => qb.QuestionBankId, Guid.NewGuid())
            .RuleFor(qb => qb.IsActive, true)
            .Generate();

        _mockWorkspaceRepository.Setup(x => x.ExistsAsync(workspaceId)).ReturnsAsync(true);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(workspaceId)).ReturnsAsync(new List<Map>());
        _mockQuestionBankRepository.Setup(x => x.GetQuestionBanksByWorkspaceId(workspaceId)).ReturnsAsync(new List<QuestionBank> { activeQuestionBank });

        // Act
        var result = await _workspaceService.Delete(workspaceId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error =>
            {
                error.Type.Should().Be(ErrorType.Validation);
                error.Code.Should().Be("Workspace.HasQuestionBanks");
            }
        );
    }

    #endregion

    #region GetByOrganization Tests

    [Fact]
    public async Task GetByOrganization_WithValidOrgId_ShouldReturnWorkspaces()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var workspaces = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.OrgId, orgId)
            .RuleFor(w => w.WorkspaceName, f => f.Company.CompanyName())
            .RuleFor(w => w.Access, WorkspaceAccessEnum.AllMembers)
            .RuleFor(w => w.IsActive, true)
            .RuleFor(w => w.CreatedAt, DateTime.UtcNow)
            .RuleFor(w => w.Organization, new DomainOrganization.Organization { OrgName = "Test Org" })
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate(2);

        _mockWorkspaceRepository.Setup(x => x.GetByOrganizationIdAsync(orgId)).ReturnsAsync(workspaces);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Map>());

        // Act
        var result = await _workspaceService.GetByOrganization(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspaces.Should().HaveCount(2);
        response.Workspaces.All(w => w.OrgId == orgId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByOrganization_WithNoWorkspaces_ShouldReturnEmptyList()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.GetByOrganizationIdAsync(orgId)).ReturnsAsync(new List<DomainWorkspace.Workspace>());

        // Act
        var result = await _workspaceService.GetByOrganization(orgId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspaces.Should().BeEmpty();
    }

    #endregion

    #region GetMyWorkspaces Tests

    [Fact]
    public async Task GetMyWorkspaces_WithValidUser_ShouldReturnWorkspaces()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workspaces = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.CreatedBy, userId)
            .RuleFor(w => w.WorkspaceName, f => f.Company.CompanyName())
            .RuleFor(w => w.Access, WorkspaceAccessEnum.AllMembers)
            .RuleFor(w => w.IsActive, true)
            .RuleFor(w => w.CreatedAt, DateTime.UtcNow)
            .RuleFor(w => w.Organization, new DomainOrganization.Organization { OrgName = "Test Org" })
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate(2);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockWorkspaceRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(workspaces);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Map>());

        // Act
        var result = await _workspaceService.GetMyWorkspaces();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Workspaces.Should().HaveCount(2);
    }

    #endregion

    #region GetWorkspaceMaps Tests

    [Fact]
    public async Task GetWorkspaceMaps_WithValidWorkspace_ShouldReturnMaps()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate();

        var maps = new Faker<Map>()
            .RuleFor(m => m.MapId, Guid.NewGuid())
            .RuleFor(m => m.MapName, f => f.Lorem.Word())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.IsPublic, false)
            .RuleFor(m => m.IsActive, true)
            .RuleFor(m => m.CreatedAt, DateTime.UtcNow)
            .Generate(3);

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetByWorkspaceIdAsync(workspaceId)).ReturnsAsync(maps);

        // Act
        var result = await _workspaceService.GetWorkspaceMaps(workspaceId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Maps.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetWorkspaceMaps_WithNonExistentWorkspace_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync((DomainWorkspace.Workspace?)null);

        // Act
        var result = await _workspaceService.GetWorkspaceMaps(workspaceId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region AddMapToWorkspace Tests

    [Fact]
    public async Task AddMapToWorkspace_WithValidData_ShouldSucceed()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, workspaceId)
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .Generate();

        var map = new Faker<Map>()
            .RuleFor(m => m.MapId, mapId)
            .RuleFor(m => m.MapName, "Test Map")
            .RuleFor(m => m.WorkspaceId, (Guid?)null)
            .Generate();

        var request = new AddMapToWorkspaceReqDto { MapId = mapId };

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetMapById(mapId)).ReturnsAsync(map);
        _mockMapRepository.Setup(x => x.UpdateMap(It.IsAny<Map>())).ReturnsAsync(true);

        // Act
        var result = await _workspaceService.AddMapToWorkspace(workspaceId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        map.WorkspaceId.Should().Be(workspaceId);
        _mockMapRepository.Verify(x => x.UpdateMap(map), Times.Once);
    }

    [Fact]
    public async Task AddMapToWorkspace_WithNonExistentWorkspace_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var request = new AddMapToWorkspaceReqDto { MapId = Guid.NewGuid() };

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync((DomainWorkspace.Workspace?)null);

        // Act
        var result = await _workspaceService.AddMapToWorkspace(workspaceId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task AddMapToWorkspace_WithNonExistentMap_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate();

        var request = new AddMapToWorkspaceReqDto { MapId = mapId };

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetMapById(mapId)).ReturnsAsync((Map?)null);

        // Act
        var result = await _workspaceService.AddMapToWorkspace(workspaceId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region RemoveMapFromWorkspace Tests

    [Fact]
    public async Task RemoveMapFromWorkspace_WithValidData_ShouldSucceed()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate();

        var map = new Faker<Map>()
            .RuleFor(m => m.MapId, mapId)
            .RuleFor(m => m.WorkspaceId, workspaceId)
            .Generate();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetMapById(mapId)).ReturnsAsync(map);
        _mockMapRepository.Setup(x => x.UpdateMap(It.IsAny<Map>())).ReturnsAsync(true);

        // Act
        var result = await _workspaceService.RemoveMapFromWorkspace(workspaceId, mapId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        map.WorkspaceId.Should().BeNull();
        _mockMapRepository.Verify(x => x.UpdateMap(map), Times.Once);
    }

    [Fact]
    public async Task RemoveMapFromWorkspace_WithNonExistentWorkspace_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var mapId = Guid.NewGuid();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync((DomainWorkspace.Workspace?)null);

        // Act
        var result = await _workspaceService.RemoveMapFromWorkspace(workspaceId, mapId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task RemoveMapFromWorkspace_WithNonExistentMap_ShouldReturnError()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var workspace = new Faker<DomainWorkspace.Workspace>()
            .RuleFor(w => w.WorkspaceId, f => f.Random.Guid())
            .RuleFor(w => w.WorkspaceName, "Test Workspace")
            .RuleFor(w => w.Creator, new DomainUser.User { FullName = "Test User" })
            .Generate();

        _mockWorkspaceRepository.Setup(x => x.GetByIdAsync(workspaceId)).ReturnsAsync(workspace);
        _mockMapRepository.Setup(x => x.GetMapById(mapId)).ReturnsAsync((Map?)null);

        // Act
        var result = await _workspaceService.RemoveMapFromWorkspace(workspaceId, mapId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion
}

