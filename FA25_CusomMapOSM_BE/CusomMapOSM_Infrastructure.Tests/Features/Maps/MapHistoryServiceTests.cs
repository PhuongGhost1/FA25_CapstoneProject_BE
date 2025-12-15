using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.Maps;
using CusomMapOSM_Application.Interfaces.Services.Organization;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Features.Maps;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Maps;

public class MapHistoryServiceTests
{
    private readonly Mock<IMapHistoryStore> _mockStore;
    private readonly MapHistoryService _mapHistoryService;
    private readonly Mock<IOrganizationPermissionService> _mockOrganizationPermissionService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Faker _faker;

    public MapHistoryServiceTests()
    {
        _mockStore = new Mock<IMapHistoryStore>();
        _mockOrganizationPermissionService = new Mock<IOrganizationPermissionService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mapHistoryService = new MapHistoryService(
            _mockStore.Object,
            _mockOrganizationPermissionService.Object,
            _mockCurrentUserService.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task RecordSnapshot_WithValidData_ShouldSucceed()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"features\":[]}";

        _mockStore.Setup(x => x.AddAsync(It.IsAny<MapHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _mapHistoryService.RecordSnapshot(mapId, userId, snapshotJson);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockStore.Verify(x => x.AddAsync(It.Is<MapHistory>(h =>
            h.MapId == mapId &&
            h.UserId == userId &&
            h.SnapshotData == snapshotJson), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordSnapshot_WithEmptySnapshot_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snapshotJson = "";

        // Act
        var result = await _mapHistoryService.RecordSnapshot(mapId, userId, snapshotJson);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task RecordSnapshot_WithWhitespaceSnapshot_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snapshotJson = "   ";

        // Act
        var result = await _mapHistoryService.RecordSnapshot(mapId, userId, snapshotJson);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task RecordSnapshot_WithNullSnapshot_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string? snapshotJson = null;

        // Act
        var result = await _mapHistoryService.RecordSnapshot(mapId, userId, snapshotJson!);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Undo_WithValidSteps_ShouldReturnSnapshot()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 2;
        var history = new List<MapHistory>
        {
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[1]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow.AddMinutes(-10))
                .Generate(),
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[1,2]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow.AddMinutes(-5))
                .Generate(),
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[1,2,3]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow)
                .Generate()
        };

        _mockStore.Setup(x => x.GetLastAsync(mapId, steps, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeTrue();
        // Undo with 2 steps: ordered[1] is the middle snapshot (second newest)
        result.ValueOrFailure().Should().Be("{\"features\":[1,2]}");
    }

    [Fact]
    public async Task Undo_WithZeroSteps_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 0;

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Undo_WithNegativeSteps_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = -1;

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Undo_WithStepsGreaterThanMax_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 11; // MaxHistory is 10

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Undo_WithInsufficientHistory_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 5;
        var history = new List<MapHistory>
        {
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow)
                .Generate()
        };

        _mockStore.Setup(x => x.GetLastAsync(mapId, steps, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task Undo_WithEmptyHistory_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 1;

        _mockStore.Setup(x => x.GetLastAsync(mapId, steps, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MapHistory>());

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task Undo_WithOneStep_ShouldReturnOldestSnapshot()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 1;
        var history = new List<MapHistory>
        {
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[1]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow.AddMinutes(-10))
                .Generate(),
            new Faker<MapHistory>()
                .RuleFor(h => h.MapId, mapId)
                .RuleFor(h => h.UserId, userId)
                .RuleFor(h => h.SnapshotData, "{\"features\":[1,2]}")
                .RuleFor(h => h.CreatedAt, DateTime.UtcNow)
                .Generate()
        };

        _mockStore.Setup(x => x.GetLastAsync(mapId, steps, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeTrue();
        // Undo with 1 step: ordered[0] is the newest snapshot (one step back from current)
        result.ValueOrFailure().Should().Be("{\"features\":[1,2]}");
    }

    [Fact]
    public async Task Undo_WithMaxSteps_ShouldReturnOldestSnapshot()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var steps = 10; // MaxHistory
        var history = new Faker<MapHistory>()
            .RuleFor(h => h.MapId, mapId)
            .RuleFor(h => h.UserId, userId)
            .RuleFor(h => h.SnapshotData, (f, h) => $"{{\"features\":[{f.Random.Int()}]}}")
            .RuleFor(h => h.CreatedAt, (f, h) => f.Date.Past())
            .Generate(10);

        _mockStore.Setup(x => x.GetLastAsync(mapId, steps, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _mapHistoryService.Undo(mapId, userId, steps);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecordSnapshot_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"features\":[]}";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockStore.Setup(x => x.AddAsync(It.IsAny<MapHistory>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mapHistoryService.RecordSnapshot(mapId, userId, snapshotJson, cancellationTokenSource.Token));
    }
}

