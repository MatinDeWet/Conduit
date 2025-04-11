using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.NotificationTests;

public class NotificationWithoutHandlerTests
{
    public class NotificationWithoutHandlerRequest : INotification
    {
        public string Data { get; set; } = null!;
    }

    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationWithoutHandlerTests()
    {

        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<NotificationWithoutHandlerTests>();
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _notificationDispatcher = serviceProvider.GetRequiredService<INotificationDispatcher>();
    }

    // Tests for Notification without handler

    [Fact]
    public async Task Publish_WhenNotificationWithoutHandler_ShouldNotThrowException()
    {
        // Arrange
        var notification = new NotificationWithoutHandlerRequest
        {
            Data = "Test"
        };

        // Act & Assert (should not throw)
        await Should.NotThrowAsync(async () => await _notificationDispatcher.Publish(notification));
    }

    [Fact]
    public async Task Publish_WhenNotificationWithoutHandlerWithNullData_ShouldNotThrowException()
    {
        // Arrange
        var notification = new NotificationWithoutHandlerRequest
        {
            Data = null!
        };

        // Act & Assert (should not throw)
        await Should.NotThrowAsync(async () => await _notificationDispatcher.Publish(notification));
    }

    // Test cancellation token behavior

    [Fact]
    public async Task Publish_WhenNotificationWithoutHandlerAndCancellationToken_ShouldNotThrowException()
    {
        // Arrange
        var notification = new NotificationWithoutHandlerRequest { Data = "Test" };
        var cancellationToken = new CancellationToken();

        // Act & Assert (should not throw)
        await Should.NotThrowAsync(async () => await _notificationDispatcher.Publish(notification, cancellationToken));
    }

    [Fact]
    public async Task Publish_WhenNotificationWithoutHandlerAndCancellationTokenCanceled_ShouldNotThrowException()
    {
        // Arrange
        var notification = new NotificationWithoutHandlerRequest { Data = "Test" };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        // For notifications without handlers, even with a canceled token,
        // we shouldn't expect an exception as there's no actual work being done
        await Should.NotThrowAsync(
            async () => await _notificationDispatcher.Publish(notification, cancellationToken));
    }

    // Test null notification

    [Fact]
    public async Task Publish_WhenNotificationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        NotificationWithoutHandlerRequest notification = null!;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            async () => await _notificationDispatcher.Publish(notification));
    }
}
