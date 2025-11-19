using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Interfaces.Services.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Features.Maps;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Tests.Features.Maps;

public class MapFeatureServiceTests
{
    private readonly Mock<IMapFeatureRepository> _mockRepository;
    private readonly Mock<IMapFeatureStore> _mockMongoStore;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapHistoryService> _mockMapHistoryService;
    private readonly MapFeatureService _mapFeatureService;
    private readonly Faker _faker;

    public MapFeatureServiceTests()
    {
        _mockRepository = new Mock<IMapFeatureRepository>();
        _mockMongoStore = new Mock<IMapFeatureStore>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockMapHistoryService = new Mock<IMapHistoryService>();
        _mapFeatureService = new MapFeatureService(
            _mockRepository.Object,
            _mockMongoStore.Object,
            _mockCurrentUserService.Object,
            _mockMapHistoryService.Object
        );
        _faker = new Faker();
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var layerId = Guid.NewGuid();
        var featureId = Guid.NewGuid();

        var request = new Faker<CreateMapFeatureRequest>()
            .RuleFor(r => r.MapId, mapId)
            .RuleFor(r => r.LayerId, layerId)
            .RuleFor(r => r.Name, f => f.Lorem.Word())
            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
            .RuleFor(r => r.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(r => r.AnnotationType, (AnnotationTypeEnum?)null)
            .RuleFor(r => r.GeometryType, GeometryTypeEnum.Point)
            .RuleFor(r => r.Coordinates, "[100.0, 50.0]")
            .RuleFor(r => r.Properties, (string?)null)
            .RuleFor(r => r.Style, (string?)null)
            .RuleFor(r => r.IsVisible, true)
            .RuleFor(r => r.ZIndex, 0)
            .Generate();

        var createdFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.LayerId, layerId)
            .RuleFor(f => f.Name, request.Name)
            .RuleFor(f => f.Description, request.Description)
            .RuleFor(f => f.FeatureCategory, request.FeatureCategory)
            .RuleFor(f => f.AnnotationType, request.AnnotationType)
            .RuleFor(f => f.GeometryType, request.GeometryType)
            .RuleFor(f => f.CreatedBy, userId)
            .RuleFor(f => f.CreatedAt, DateTime.UtcNow)
            .RuleFor(f => f.IsVisible, true)
            .RuleFor(f => f.ZIndex, 0)
            .Generate();

        var mongoDoc = new MapFeatureDocument
        {
            Id = featureId.ToString(),
            MapId = mapId,
            LayerId = layerId,
            Geometry = JsonSerializer.Deserialize<object>("{\"type\":\"Point\",\"coordinates\":[100.0,50.0]}")
        };

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockMongoStore.Setup(x => x.CreateAsync(It.IsAny<MapFeatureDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureId.ToString());
        _mockRepository.Setup(x => x.Create(It.IsAny<MapFeature>())).ReturnsAsync(featureId);
        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));
        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(createdFeature);
        _mockMongoStore.Setup(x => x.GetAsync(featureId, It.IsAny<CancellationToken>())).ReturnsAsync(mongoDoc);

        // Act
        var result = await _mapFeatureService.Create(request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.FeatureId.Should().Be(featureId);
        response.MapId.Should().Be(mapId);
        _mockRepository.Verify(x => x.Create(It.IsAny<MapFeature>()), Times.Once);
        _mockMongoStore.Verify(x => x.CreateAsync(It.IsAny<MapFeatureDocument>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithUnauthorizedUser_ShouldReturnError()
    {
        // Arrange
        var request = new Faker<CreateMapFeatureRequest>()
            .RuleFor(r => r.MapId, Guid.NewGuid())
            .RuleFor(r => r.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(r => r.GeometryType, GeometryTypeEnum.Point)
            .RuleFor(r => r.Coordinates, "[100.0, 50.0]")
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns((Guid?)null);

        // Act
        var result = await _mapFeatureService.Create(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Unauthorized)
        );
    }

    [Fact]
    public async Task Create_WithInvalidGeometry_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new Faker<CreateMapFeatureRequest>()
            .RuleFor(r => r.MapId, Guid.NewGuid())
            .RuleFor(r => r.FeatureCategory, FeatureCategoryEnum.Annotation)
            .RuleFor(r => r.AnnotationType, AnnotationTypeEnum.Marker)
            .RuleFor(r => r.GeometryType, GeometryTypeEnum.Polygon) // Invalid: Marker should be Point or Circle
            .RuleFor(r => r.Coordinates, "[100.0, 50.0]")
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);

        // Act
        var result = await _mapFeatureService.Create(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Create_WithRepositoryFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var featureId = Guid.NewGuid();

        var request = new Faker<CreateMapFeatureRequest>()
            .RuleFor(r => r.MapId, mapId)
            .RuleFor(r => r.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(r => r.GeometryType, GeometryTypeEnum.Point)
            .RuleFor(r => r.Coordinates, "[100.0, 50.0]")
            .Generate();

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockMongoStore.Setup(x => x.CreateAsync(It.IsAny<MapFeatureDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureId.ToString());
        _mockRepository.Setup(x => x.Create(It.IsAny<MapFeature>())).ReturnsAsync(featureId);
        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));
        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync((MapFeature?)null);

        // Act
        var result = await _mapFeatureService.Create(request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var layerId = Guid.NewGuid();

        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.LayerId, layerId)
            .RuleFor(f => f.Name, "Old Name")
            .RuleFor(f => f.Description, "Old Description")
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .RuleFor(f => f.IsVisible, true)
            .RuleFor(f => f.ZIndex, 0)
            .Generate();

        var request = new Faker<UpdateMapFeatureRequest>()
            .RuleFor(r => r.Name, "New Name")
            .RuleFor(r => r.Description, "New Description")
            .RuleFor(r => r.Coordinates, (string?)null)
            .RuleFor(r => r.Properties, (string?)null)
            .RuleFor(r => r.Style, (string?)null)
            .Generate();

        var mongoDoc = new MapFeatureDocument
        {
            Id = featureId.ToString(),
            MapId = mapId,
            Geometry = JsonSerializer.Deserialize<object>("{\"type\":\"Point\",\"coordinates\":[100.0,50.0]}")
        };

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);
        _mockRepository.Setup(x => x.Update(It.IsAny<MapFeature>())).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));
        _mockMongoStore.Setup(x => x.GetAsync(featureId, It.IsAny<CancellationToken>())).ReturnsAsync(mongoDoc);

        // Act
        var result = await _mapFeatureService.Update(featureId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Name.Should().Be("New Name");
        response.Description.Should().Be("New Description");
        _mockRepository.Verify(x => x.Update(It.IsAny<MapFeature>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistentFeature_ShouldReturnError()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var request = new UpdateMapFeatureRequest { Name = "New Name" };

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync((MapFeature?)null);

        // Act
        var result = await _mapFeatureService.Update(featureId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task Update_WithInvalidGeometry_ShouldReturnError()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Annotation)
            .RuleFor(f => f.AnnotationType, AnnotationTypeEnum.Marker)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate();

        var request = new UpdateMapFeatureRequest
        {
            GeometryType = GeometryTypeEnum.Polygon // Invalid for Marker
        };

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);

        // Act
        var result = await _mapFeatureService.Update(featureId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task Update_WithCoordinatesUpdate_ShouldUpdateMongoDoc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mapId = Guid.NewGuid();

        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate();

        var request = new UpdateMapFeatureRequest
        {
            Coordinates = "[200.0, 100.0]"
        };

        var mongoDoc = new MapFeatureDocument
        {
            Id = featureId.ToString(),
            MapId = mapId,
            Geometry = JsonSerializer.Deserialize<object>("{\"type\":\"Point\",\"coordinates\":[100.0,50.0]}")
        };

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);
        _mockMongoStore.Setup(x => x.GetAsync(featureId, It.IsAny<CancellationToken>())).ReturnsAsync(mongoDoc);
        _mockMongoStore.Setup(x => x.UpdateAsync(It.IsAny<MapFeatureDocument>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.Update(It.IsAny<MapFeature>())).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _mapFeatureService.Update(featureId, request);

        // Assert
        result.HasValue.Should().BeTrue();
        _mockMongoStore.Verify(x => x.UpdateAsync(It.IsAny<MapFeatureDocument>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithRepositoryFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mapId = Guid.NewGuid();

        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate();

        var request = new UpdateMapFeatureRequest { Name = "New Name" };

        var mongoDoc = new MapFeatureDocument { Id = featureId.ToString() };

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);
        _mockRepository.Setup(x => x.Update(It.IsAny<MapFeature>())).ReturnsAsync(false);
        _mockMongoStore.Setup(x => x.GetAsync(featureId, It.IsAny<CancellationToken>())).ReturnsAsync(mongoDoc);

        // Act
        var result = await _mapFeatureService.Update(featureId, request);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidFeature_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mapId = Guid.NewGuid();

        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .Generate();

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);
        _mockMongoStore.Setup(x => x.DeleteAsync(featureId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.Delete(featureId)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _mapFeatureService.Delete(featureId);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockRepository.Verify(x => x.Delete(featureId), Times.Once);
        _mockMongoStore.Verify(x => x.DeleteAsync(featureId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentFeature_ShouldReturnError()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync((MapFeature?)null);

        // Act
        var result = await _mapFeatureService.Delete(featureId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task Delete_WithRepositoryFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mapId = Guid.NewGuid();

        var existingFeature = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, featureId)
            .RuleFor(f => f.MapId, mapId)
            .Generate();

        _mockRepository.Setup(x => x.GetById(featureId)).ReturnsAsync(existingFeature);
        _mockMongoStore.Setup(x => x.DeleteAsync(featureId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.Delete(featureId)).ReturnsAsync(false);

        // Act
        var result = await _mapFeatureService.Delete(featureId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region GetByMap Tests

    [Fact]
    public async Task GetByMap_WithValidMapId_ShouldReturnFeatures()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var features = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, f => f.Random.Guid())
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate(3);

        var mongoDocs = features.Select(f => new MapFeatureDocument
        {
            Id = f.FeatureId.ToString(),
            MapId = mapId,
            Geometry = JsonSerializer.Deserialize<object>("{\"type\":\"Point\",\"coordinates\":[100.0,50.0]}")
        }).ToList();

        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(features);
        _mockMongoStore.Setup(x => x.GetByMapAsync(mapId, It.IsAny<CancellationToken>())).ReturnsAsync(mongoDocs);

        // Act
        var result = await _mapFeatureService.GetByMap(mapId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByMap_WithEmptyMap_ShouldReturnEmptyList()
    {
        // Arrange
        var mapId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByMap(mapId)).ReturnsAsync(new List<MapFeature>());
        _mockMongoStore.Setup(x => x.GetByMapAsync(mapId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<MapFeatureDocument>());

        // Act
        var result = await _mapFeatureService.GetByMap(mapId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().BeEmpty();
    }

    #endregion

    #region GetByMapAndCategory Tests

    [Fact]
    public async Task GetByMapAndCategory_WithValidData_ShouldReturnFeatures()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var category = FeatureCategoryEnum.Data;
        var features = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, f => f.Random.Guid())
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.FeatureCategory, category)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate(2);

        _mockRepository.Setup(x => x.GetByMapAndCategory(mapId, category)).ReturnsAsync(features);

        // Act
        var result = await _mapFeatureService.GetByMapAndCategory(mapId, category);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().HaveCount(2);
        response.All(f => f.FeatureCategory == category).Should().BeTrue();
    }

    [Fact]
    public async Task GetByMapAndCategory_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var category = FeatureCategoryEnum.Annotation;

        _mockRepository.Setup(x => x.GetByMapAndCategory(mapId, category)).ReturnsAsync(new List<MapFeature>());

        // Act
        var result = await _mapFeatureService.GetByMapAndCategory(mapId, category);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().BeEmpty();
    }

    #endregion

    #region GetByMapAndLayer Tests

    [Fact]
    public async Task GetByMapAndLayer_WithValidData_ShouldReturnFeatures()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var layerId = Guid.NewGuid();
        var features = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, f => f.Random.Guid())
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.LayerId, layerId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate(2);

        _mockRepository.Setup(x => x.GetByMapAndLayer(mapId, layerId)).ReturnsAsync(features);

        // Act
        var result = await _mapFeatureService.GetByMapAndLayer(mapId, layerId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().HaveCount(2);
        response.All(f => f.LayerId == layerId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByMapAndLayer_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var layerId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByMapAndLayer(mapId, layerId)).ReturnsAsync(new List<MapFeature>());

        // Act
        var result = await _mapFeatureService.GetByMapAndLayer(mapId, layerId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Should().BeEmpty();
    }

    #endregion

    #region ApplySnapshot Tests

    [Fact]
    public async Task ApplySnapshot_WithValidSnapshot_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var features = new Faker<MapFeature>()
            .RuleFor(f => f.FeatureId, f => f.Random.Guid())
            .RuleFor(f => f.MapId, mapId)
            .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
            .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
            .Generate(3);

        var snapshotJson = JsonSerializer.Serialize(features);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockRepository.Setup(x => x.DeleteByMap(mapId)).ReturnsAsync(3);
        _mockRepository.Setup(x => x.AddRange(It.IsAny<IEnumerable<MapFeature>>())).ReturnsAsync(3);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _mapFeatureService.ApplySnapshot(mapId, snapshotJson);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteByMap(mapId), Times.Once);
        _mockRepository.Verify(x => x.AddRange(It.IsAny<IEnumerable<MapFeature>>()), Times.Once);
    }

    [Fact]
    public async Task ApplySnapshot_WithUnauthorizedUser_ShouldReturnError()
    {
        // Arrange
        var mapId = Guid.NewGuid();
        var snapshotJson = "[]";

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns((Guid?)null);

        // Act
        var result = await _mapFeatureService.ApplySnapshot(mapId, snapshotJson);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Unauthorized)
        );
    }

    [Fact]
    public async Task ApplySnapshot_WithInvalidJson_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var snapshotJson = "invalid json";

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);

        // Act
        var result = await _mapFeatureService.ApplySnapshot(mapId, snapshotJson);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().BeOneOf(ErrorType.Validation, ErrorType.Failure)
        );
    }

    [Fact]
    public async Task ApplySnapshot_WithEmptyFeatures_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var snapshotJson = "[]";

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockRepository.Setup(x => x.DeleteByMap(mapId)).ReturnsAsync(0);
        _mockRepository.Setup(x => x.AddRange(It.IsAny<IEnumerable<MapFeature>>())).ReturnsAsync(0);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _mapFeatureService.ApplySnapshot(mapId, snapshotJson);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
    }

    [Fact]
    public async Task ApplySnapshot_WithFeaturesHavingEmptyGuid_ShouldGenerateNewGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mapId = Guid.NewGuid();
        var features = new List<MapFeature>
        {
            new Faker<MapFeature>()
                .RuleFor(f => f.FeatureId, Guid.Empty)
                .RuleFor(f => f.MapId, Guid.Empty)
                .RuleFor(f => f.FeatureCategory, FeatureCategoryEnum.Data)
                .RuleFor(f => f.GeometryType, GeometryTypeEnum.Point)
                .Generate()
        };

        var snapshotJson = JsonSerializer.Serialize(features);

        _mockCurrentUserService.Setup(x => x.GetUserId()).Returns(userId);
        _mockRepository.Setup(x => x.DeleteByMap(mapId)).ReturnsAsync(0);
        _mockRepository.Setup(x => x.AddRange(It.IsAny<IEnumerable<MapFeature>>())).ReturnsAsync(1);
        _mockMapHistoryService.Setup(x => x.RecordSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Option.Some<bool, Error>(true));

        // Act
        var result = await _mapFeatureService.ApplySnapshot(mapId, snapshotJson);

        // Assert
        result.HasValue.Should().BeTrue();
        _mockRepository.Verify(x => x.AddRange(It.Is<IEnumerable<MapFeature>>(f => 
            f.All(feature => feature.FeatureId != Guid.Empty && feature.MapId == mapId))), Times.Once);
    }

    #endregion
}

