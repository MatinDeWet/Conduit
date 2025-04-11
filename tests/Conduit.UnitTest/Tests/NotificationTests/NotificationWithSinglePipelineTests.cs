using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.NotificationTests;

public class NotificationWithSinglePipelineTests
{
    public static readonly List<string> PipelineExecutionOrder = [];

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
            PipelineExecutionOrder.Add(nameof(TestNotificationHandler));
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            InvocationCount = 0;
            LastReceivedData = null;
        }
    }

    public class NotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public static int InvocationCount { get; private set; }

        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(NotificationPipelineBehavior<TNotification>));
            return next();
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationWithSinglePipelineTests()
    {
        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<NotificationWithSinglePipelineTests>();
            config.AddNotificationBehavior(typeof(NotificationPipelineBehavior<>));
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _notificationDispatcher = serviceProvider.GetRequiredService<INotificationDispatcher>();
    }

    private static void ResetAllCounters()
    {
        TestNotificationHandler.Reset();
        NotificationPipelineBehavior<TestNotification>.ResetInvocationCount();
        PipelineExecutionOrder.Clear();
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldInvokeHandler()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldInvokePipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        NotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldExecutePipelineBeforeHandler()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(2);
        PipelineExecutionOrder[0].ShouldBe(nameof(NotificationPipelineBehavior<TestNotification>));
        PipelineExecutionOrder[1].ShouldBe(nameof(TestNotificationHandler));
    }

    [Fact]
    public async Task Publish_WhenNotificationCalledMultipleTimes_ShouldIncrementHandlerInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(2);
    }

    [Fact]
    public async Task Publish_WhenNotificationCalledMultipleTimes_ShouldIncrementPipelineInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);
        await _notificationDispatcher.Publish(notification);

        // Assert
        NotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(2);
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldPassCorrectDataToHandler()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.LastReceivedData.ShouldBe("TestData");
    }

    [Fact]
    public async Task Publish_WhenNotificationWithDifferentData_ShouldPassCorrectDataToHandler()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "DifferentData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.LastReceivedData.ShouldBe("DifferentData");
    }

    [Fact]
    public async Task Publish_WithCancellationToken_ShouldInvokeHandlersAndPipeline()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };
        var cancellationToken = new CancellationToken();

        // Act
        await _notificationDispatcher.Publish(notification, cancellationToken);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
        NotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await _notificationDispatcher.Publish(notification, cancellationToken));

        // Handlers and pipeline should not be invoked if token is canceled
        TestNotificationHandler.InvocationCount.ShouldBe(0);
        NotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(0);
    }
}
