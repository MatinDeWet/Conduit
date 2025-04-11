using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.NotificationTests;

public class NotificationWithHandlerTests
{
    public class TestNotification : INotification
    {
        public string Data { get; set; } = null!;
    }

    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public static int InvocationCount { get; private set; }
        public static string? LastReceivedData { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvocationCount++;
            LastReceivedData = notification.Data;

            return Task.CompletedTask;
        }

        public static void Reset()
        {
            InvocationCount = 0;
            LastReceivedData = null;
        }
    }

    public class SecondTestNotificationHandler : INotificationHandler<TestNotification>
    {
        public static int InvocationCount { get; private set; }
        public static string? LastReceivedData { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvocationCount++;
            LastReceivedData = notification.Data;

            return Task.CompletedTask;
        }

        public static void Reset()
        {
            InvocationCount = 0;
            LastReceivedData = null;
        }
    }

    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationWithHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<NotificationWithHandlerTests>();
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _notificationDispatcher = serviceProvider.GetRequiredService<INotificationDispatcher>();
    }

    private static void ResetAllHandlers()
    {
        TestNotificationHandler.Reset();
        SecondTestNotificationHandler.Reset();
    }

    [Fact]
    public async Task Publish_WhenNotificationHasHandler_ShouldCallDispatcher()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert - This test just confirms the call completes without errors
    }

    [Fact]
    public async Task Publish_WhenNotificationHasHandler_ShouldInvokeHandlerExactlyOnce()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotificationHasHandler_ShouldPassCorrectData()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.LastReceivedData.ShouldBe("TestData");
    }

    [Fact]
    public async Task Publish_WhenNotificationHasMultipleHandlers_ShouldInvokeAllHandlers()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
        SecondTestNotificationHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotificationCalledMultipleTimes_ShouldInvokeHandlerMultipleTimes()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);
        await _notificationDispatcher.Publish(notification);
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(3);
        SecondTestNotificationHandler.InvocationCount.ShouldBe(3);
    }

    [Fact]
    public async Task Publish_WhenNotificationHasMultipleHandlersWithDifferentData_ShouldPassCorrectDataToAll()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "DifferentData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.LastReceivedData.ShouldBe("DifferentData");
        SecondTestNotificationHandler.LastReceivedData.ShouldBe("DifferentData");
    }

    [Fact]
    public async Task Publish_WithCancellationToken_ShouldInvokeHandlers()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };
        var cancellationToken = new CancellationToken();

        // Act
        await _notificationDispatcher.Publish(notification, cancellationToken);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
        SecondTestNotificationHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        ResetAllHandlers();
        var notification = new TestNotification { Data = "TestData" };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await _notificationDispatcher.Publish(notification, cancellationToken));

        // Handlers should not be invoked if token is canceled
        TestNotificationHandler.InvocationCount.ShouldBe(0);
        SecondTestNotificationHandler.InvocationCount.ShouldBe(0);
    }

    [Fact]
    public async Task Publish_WithNullNotification_ShouldThrowArgumentNullException()
    {
        // Arrange
        ResetAllHandlers();
        TestNotification notification = null!;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            async () => await _notificationDispatcher.Publish(notification));
    }
}
