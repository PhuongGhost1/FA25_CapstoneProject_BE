using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;
using CusomMapOSM_Infrastructure.Features.Animations;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Animations;

public class LayerAnimationServiceTests
{
    private readonly Mock<ILayerAnimationRepository> _mockRepository;
    private readonly LayerAnimationService _layerAnimationService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IFirebaseStorageService> _mockFirebaseStorageService;
    private readonly Faker _faker;

    public LayerAnimationServiceTests(Mock<IFirebaseStorageService> mockFirebaseStorageService)
    {
        _mockFirebaseStorageService = mockFirebaseStorageService;
        _mockRepository = new Mock<ILayerAnimationRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _layerAnimationService = new LayerAnimationService(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockFirebaseStorageService.Object
            );
        _faker = new Faker();
    }

    #region GetAnimationsByLayerAsync Tests

    [Fact]
    public async Task GetAnimationsByLayerAsync_WithValidLayerId_ShouldReturnAnimations()
    {
        // Arrange
        var layerId = Guid.NewGuid();
        var animations = new Faker<AnimatedLayer>()
            .RuleFor(a => a.AnimatedLayerId, Guid.NewGuid())
            .RuleFor(a => a.LayerId, layerId)
            .RuleFor(a => a.Name, f => f.Lorem.Word())
            .RuleFor(a => a.SourceUrl, f => f.Internet.Url())
            .RuleFor(a => a.Coordinates, "[100.0, 50.0]")
            .RuleFor(a => a.RotationDeg, 0)
            .RuleFor(a => a.Scale, 1.0)
            .RuleFor(a => a.ZIndex, 0)
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow)
            .RuleFor(a => a.IsVisible, true)
            .Generate(3);

        _mockRepository.Setup(x => x.GetAnimationsByLayerAsync(layerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animations);

        // Act
        var result = await _layerAnimationService.GetAnimationsByLayerAsync(layerId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().HaveCount(3);
        response.All(a => a.LayerId == layerId).Should().BeTrue();
    }

    [Fact]
    public async Task GetAnimationsByLayerAsync_WithNoAnimations_ShouldReturnEmptyList()
    {
        // Arrange
        var layerId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetAnimationsByLayerAsync(layerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnimatedLayer>());

        // Act
        var result = await _layerAnimationService.GetAnimationsByLayerAsync(layerId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().BeEmpty();
    }

    #endregion

    #region GetAnimationAsync Tests

    [Fact]
    public async Task GetAnimationAsync_WithValidId_ShouldReturnAnimation()
    {
        // Arrange
        var animationId = Guid.NewGuid();
        var layerId = Guid.NewGuid();
        var animation = new Faker<AnimatedLayer>()
            .RuleFor(a => a.AnimatedLayerId, animationId)
            .RuleFor(a => a.LayerId, layerId)
            .RuleFor(a => a.Name, "Test Animation")
            .RuleFor(a => a.SourceUrl, "https://example.com/animation.gif")
            .RuleFor(a => a.Coordinates, "[100.0, 50.0]")
            .RuleFor(a => a.RotationDeg, 45.0)
            .RuleFor(a => a.Scale, 1.5)
            .RuleFor(a => a.ZIndex, 10)
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow)
            .RuleFor(a => a.IsVisible, true)
            .Generate();

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animation);

        // Act
        var result = await _layerAnimationService.GetAnimationAsync(animationId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.LayerAnimationId.Should().Be(animationId);
        response.LayerId.Should().Be(layerId);
        response.Name.Should().Be("Test Animation");
    }

    [Fact]
    public async Task GetAnimationAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var animationId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnimatedLayer?)null);

        // Act
        var result = await _layerAnimationService.GetAnimationAsync(animationId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region CreateAnimationAsync Tests

    [Fact]
    public async Task CreateAnimationAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var layerId = Guid.NewGuid();
        var request = new CreateLayerAnimationRequest(
            layerId,
            "New Animation",
            null,
            "[100.0, 50.0]",
            45.0,
            1.5,
            10
        );

        AnimatedLayer? savedEntity = null;
        _mockRepository.Setup(x => x.AddAnimationAsync(It.IsAny<AnimatedLayer>(), It.IsAny<CancellationToken>()))
            .Callback<AnimatedLayer, CancellationToken>((entity, ct) => savedEntity = entity)
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _layerAnimationService.CreateAnimationAsync(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.LayerId.Should().Be(layerId);
        response.Name.Should().Be("New Animation");
        response.SourceUrl.Should().Be("https://example.com/animation.gif");
        response.IsActive.Should().BeTrue();
        _mockRepository.Verify(x => x.AddAnimationAsync(It.IsAny<AnimatedLayer>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAnimationAsync_WithNullCoordinates_ShouldSucceed()
    {
        // Arrange
        var layerId = Guid.NewGuid();
        var request = new CreateLayerAnimationRequest(
            layerId,
            "Animation",
            null,
            null,
            0.0,
            1.0,
            0
        );

        _mockRepository.Setup(x => x.AddAnimationAsync(It.IsAny<AnimatedLayer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _layerAnimationService.CreateAnimationAsync(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Coordinates.Should().BeNull();
    }

    #endregion

    #region UpdateAnimationAsync Tests

    [Fact]
    public async Task UpdateAnimationAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var animationId = Guid.NewGuid();
        var layerId = Guid.NewGuid();
        var existingAnimation = new Faker<AnimatedLayer>()
            .RuleFor(a => a.AnimatedLayerId, animationId)
            .RuleFor(a => a.LayerId, layerId)
            .RuleFor(a => a.Name, "Old Name")
            .RuleFor(a => a.SourceUrl, "https://example.com/old.gif")
            .RuleFor(a => a.Coordinates, "[100.0, 50.0]")
            .RuleFor(a => a.RotationDeg, 0.0)
            .RuleFor(a => a.Scale, 1.0)
            .RuleFor(a => a.ZIndex, 0)
            .RuleFor(a => a.IsVisible, false)
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow)
            .Generate();

        var request = new UpdateLayerAnimationRequest(
            "New Name",
            null,
            "[200.0, 100.0]",
            90.0,
            2.0,
            20,
            true
        );

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnimation);
        _mockRepository.Setup(x => x.UpdateAnimation(It.IsAny<AnimatedLayer>()));
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _layerAnimationService.UpdateAnimationAsync(animationId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Name.Should().Be("New Name");
        response.SourceUrl.Should().Be("https://example.com/new.gif");
        response.IsActive.Should().BeTrue();
        existingAnimation.Name.Should().Be("New Name");
        existingAnimation.IsVisible.Should().BeTrue();
        _mockRepository.Verify(x => x.UpdateAnimation(It.IsAny<AnimatedLayer>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAnimationAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var animationId = Guid.NewGuid();
        var request = new UpdateLayerAnimationRequest(
            "New Name",
            null,
            null,
            0.0,
            1.0,
            0,
            true
        );

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnimatedLayer?)null);

        // Act
        var result = await _layerAnimationService.UpdateAnimationAsync(animationId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region DeleteAnimationAsync Tests

    [Fact]
    public async Task DeleteAnimationAsync_WithValidId_ShouldSucceed()
    {
        // Arrange
        var animationId = Guid.NewGuid();
        var existingAnimation = new Faker<AnimatedLayer>()
            .RuleFor(a => a.AnimatedLayerId, animationId)
            .RuleFor(a => a.LayerId, Guid.NewGuid())
            .RuleFor(a => a.Name, "Test Animation")
            .Generate();

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnimation);
        _mockRepository.Setup(x => x.RemoveAnimation(It.IsAny<AnimatedLayer>()));
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _layerAnimationService.DeleteAnimationAsync(animationId);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockRepository.Verify(x => x.RemoveAnimation(existingAnimation), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAnimationAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var animationId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetAnimationAsync(animationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnimatedLayer?)null);

        // Act
        var result = await _layerAnimationService.DeleteAnimationAsync(animationId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region GetActiveAnimationsAsync Tests

    [Fact]
    public async Task GetActiveAnimationsAsync_WithActiveAnimations_ShouldReturnAnimations()
    {
        // Arrange
        var animations = new Faker<AnimatedLayer>()
            .RuleFor(a => a.AnimatedLayerId, Guid.NewGuid())
            .RuleFor(a => a.LayerId, Guid.NewGuid())
            .RuleFor(a => a.Name, f => f.Lorem.Word())
            .RuleFor(a => a.SourceUrl, f => f.Internet.Url())
            .RuleFor(a => a.Coordinates, "[100.0, 50.0]")
            .RuleFor(a => a.RotationDeg, 0)
            .RuleFor(a => a.Scale, 1.0)
            .RuleFor(a => a.ZIndex, 0)
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow)
            .RuleFor(a => a.IsVisible, true)
            .Generate(5);

        _mockRepository.Setup(x => x.GetActiveAnimationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(animations);

        // Act
        var result = await _layerAnimationService.GetActiveAnimationsAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().HaveCount(5);
        response.All(a => a.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAnimationsAsync_WithNoActiveAnimations_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetActiveAnimationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnimatedLayer>());

        // Act
        var result = await _layerAnimationService.GetActiveAnimationsAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().BeEmpty();
    }

    #endregion
}

