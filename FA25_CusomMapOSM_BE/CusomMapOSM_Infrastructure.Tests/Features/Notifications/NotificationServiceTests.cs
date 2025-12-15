using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Models.DTOs.Features.Notifications;
using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Domain.Entities.Notifications.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Notifications;
using CusomMapOSM_Infrastructure.Features.Notifications;
using CusomMapOSM_Infrastructure.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;

namespace CusomMapOSM_Infrastructure.Tests.Features.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly NotificationService _notificationService;
    private readonly Faker _faker;

    static NotificationServiceTests()
    {
        // Initialize Hangfire with in-memory storage for tests (only once, before any test runs)
        var storage = new MemoryStorage();
        GlobalConfiguration.Configuration.UseStorage(storage);
        JobStorage.Current = storage;
    }

    public NotificationServiceTests()
    {
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockClientProxy = new Mock<IClientProxy>();

        // Setup SignalR mocks
        // Note: SendAsync is an extension method and cannot be mocked directly
        // The extension method will work with the mock object, but we verify Clients.Group() calls instead
        _mockHubContext.Setup(x => x.Clients.Group(It.IsAny<string>()))
            .Returns(_mockClientProxy.Object);

        _notificationService = new NotificationService(
            _mockNotificationRepository.Object,
            _mockHubContext.Object
        );
        _faker = new Faker();
    }

    #region GetUserNotificationsAsync Tests

    [Fact]
    public async Task GetUserNotificationsAsync_WithValidUserId_ShouldReturnNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;

        var notifications = new Faker<Notification>()
            .RuleFor(n => n.NotificationId, f => f.Random.Int(1, 1000))
            .RuleFor(n => n.UserId, userId)
            .RuleFor(n => n.Type, "info")
            .RuleFor(n => n.Message, f => f.Lorem.Sentence())
            .RuleFor(n => n.Status, "sent")
            .RuleFor(n => n.CreatedAt, DateTime.UtcNow)
            .RuleFor(n => n.IsRead, false)
            .Generate(5);

        _mockNotificationRepository.Setup(x => x.GetUserNotificationsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _mockNotificationRepository.Setup(x => x.GetTotalCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Notifications.Should().HaveCount(5);
        response.TotalCount.Should().Be(10);
        response.UnreadCount.Should().Be(3);
        response.Page.Should().Be(page);
        response.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithNoNotifications_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>());

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mockNotificationRepository.Setup(x => x.GetTotalCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _notificationService.GetUserNotificationsAsync(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Notifications.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.GetUserNotificationsAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region MarkNotificationAsReadAsync Tests

    [Fact]
    public async Task MarkNotificationAsReadAsync_WithValidNotification_ShouldMarkAsRead()
    {
        // Arrange
        var notificationId = 1;
        var userId = Guid.NewGuid();

        var notification = new Faker<Notification>()
            .RuleFor(n => n.NotificationId, notificationId)
            .RuleFor(n => n.UserId, userId)
            .RuleFor(n => n.IsRead, false)
            .Generate();

        _mockNotificationRepository.Setup(x => x.GetNotificationByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _mockNotificationRepository.Setup(x => x.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        _mockNotificationRepository.Verify(x => x.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_WithNonExistentNotification_ShouldReturnError()
    {
        // Arrange
        var notificationId = 999;

        _mockNotificationRepository.Setup(x => x.GetNotificationByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_WithMarkAsReadFailure_ShouldReturnError()
    {
        // Arrange
        var notificationId = 1;
        var userId = Guid.NewGuid();

        var notification = new Faker<Notification>()
            .RuleFor(n => n.NotificationId, notificationId)
            .RuleFor(n => n.UserId, userId)
            .Generate();

        _mockNotificationRepository.Setup(x => x.GetNotificationByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _mockNotificationRepository.Setup(x => x.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    #endregion

    #region MarkAllNotificationsAsReadAsync Tests

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_WithValidUserId_ShouldMarkAllAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadCount = 5;

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unreadCount);

        _mockNotificationRepository.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Result.Should().Contain("successfully");
        response.MarkedCount.Should().Be(unreadCount);
        _mockNotificationRepository.Verify(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_WithMarkAllFailure_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _mockNotificationRepository.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region GetUnreadCountAsync Tests

    [Fact]
    public async Task GetUnreadCountAsync_WithValidUserId_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var count = 5;

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        // Act
        var result = await _notificationService.GetUnreadCountAsync(userId);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().Be(count);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.GetUnreadCountAsync(userId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CreateNotificationAsync Tests

    [Fact]
    public async Task CreateNotificationAsync_WithValidData_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "info";
        var message = "Test notification";
        var metadata = "{\"key\":\"value\"}";

        var notification = new Notification
        {
            NotificationId = 1,
            UserId = userId,
            Type = type,
            Message = message,
            Status = "pending",
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, ct) => { n.NotificationId = 1; })
            .ReturnsAsync(true);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _notificationService.CreateNotificationAsync(userId, type, message, metadata);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.Is<Notification>(n =>
            n.UserId == userId &&
            n.Type == type &&
            n.Message == message &&
            n.Metadata == metadata), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNoNotificationId_ShouldNotPushToSignalR()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "info";
        var message = "Test notification";

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, ct) => { n.NotificationId = 0; }) // No ID set
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.CreateNotificationAsync(userId, type, message);

        // Assert
        result.HasValue.Should().BeTrue();
        // SignalR should not be called when NotificationId is 0
        // Note: We verify Clients.Group() wasn't called instead since SendAsync is an extension method
        _mockHubContext.Verify(x => x.Clients.Group($"user_{userId}"), Times.Never);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.CreateNotificationAsync(userId, "info", "message");

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CreateQuotaWarningNotificationAsync Tests

    [Fact]
    public async Task CreateQuotaWarningNotificationAsync_WithNoRecentNotification_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quotaType = "maps";
        var currentUsage = 80;
        var limit = 100;
        var percentageUsed = 80;

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaWarning.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, ct) => { n.NotificationId = 1; })
            .ReturnsAsync(true);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _notificationService.CreateQuotaWarningNotificationAsync(userId, quotaType, currentUsage, limit, percentageUsed);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.Is<Notification>(n =>
            n.Type == NotificationTypeEnum.QuotaWarning.ToString() &&
            n.Message.Contains(quotaType) &&
            n.Message.Contains(percentageUsed.ToString())), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuotaWarningNotificationAsync_WithRecentNotification_ShouldReturnTrueWithoutCreating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quotaType = "maps";

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaWarning.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.CreateQuotaWarningNotificationAsync(userId, quotaType, 80, 100, 80);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuotaWarningNotificationAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.CreateQuotaWarningNotificationAsync(userId, "maps", 80, 100, 80);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CreateQuotaExceededNotificationAsync Tests

    [Fact]
    public async Task CreateQuotaExceededNotificationAsync_WithNoRecentNotification_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quotaType = "exports";
        var currentUsage = 150;
        var limit = 100;

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaExceeded.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, ct) => { n.NotificationId = 1; })
            .ReturnsAsync(true);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _notificationService.CreateQuotaExceededNotificationAsync(userId, quotaType, currentUsage, limit);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.Is<Notification>(n =>
            n.Type == NotificationTypeEnum.QuotaExceeded.ToString() &&
            n.Message.Contains(quotaType) &&
            n.Message.Contains("exceeded")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuotaExceededNotificationAsync_WithRecentNotification_ShouldReturnTrueWithoutCreating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quotaType = "exports";

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaExceeded.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.CreateQuotaExceededNotificationAsync(userId, quotaType, 150, 100);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuotaExceededNotificationAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.HasQuotaNotificationAsync(userId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.CreateQuotaExceededNotificationAsync(userId, "exports", 150, 100);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion

    #region CreateTransactionCompletedNotificationAsync Tests

    [Fact]
    public async Task CreateTransactionCompletedNotificationAsync_WithValidData_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 99.99m;
        var planName = "Pro Plan";

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, ct) => { n.NotificationId = 1; })
            .ReturnsAsync(true);

        _mockNotificationRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _notificationService.CreateTransactionCompletedNotificationAsync(userId, amount, planName);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Should().BeTrue();
        _mockNotificationRepository.Verify(x => x.CreateNotificationAsync(It.Is<Notification>(n =>
            n.Type == NotificationTypeEnum.TransactionCompleted.ToString() &&
            n.Message.Contains(amount.ToString("F2")) &&
            n.Message.Contains(planName)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionCompletedNotificationAsync_WithException_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockNotificationRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _notificationService.CreateTransactionCompletedNotificationAsync(userId, 99.99m, "Plan");

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    #endregion
}

