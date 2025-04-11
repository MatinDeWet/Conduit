using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.NotificationTests;

public class NotificationWithMultiplePipelinesTests
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

    public class FirstNotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public static int InvocationCount { get; private set; }

        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(FirstNotificationPipelineBehavior<TNotification>));
            return next();
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    public class SecondNotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public static int InvocationCount { get; private set; }

        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(SecondNotificationPipelineBehavior<TNotification>));
            return next();
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationWithMultiplePipelinesTests()
    {
        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<NotificationWithMultiplePipelinesTests>();
            config.AddNotificationBehavior(typeof(FirstNotificationPipelineBehavior<>));
            config.AddNotificationBehavior(typeof(SecondNotificationPipelineBehavior<>));
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _notificationDispatcher = serviceProvider.GetRequiredService<INotificationDispatcher>();
    }

    private static void ResetAllCounters()
    {
        TestNotificationHandler.Reset();
        FirstNotificationPipelineBehavior<TestNotification>.ResetInvocationCount();
        SecondNotificationPipelineBehavior<TestNotification>.ResetInvocationCount();
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
    public async Task Publish_WhenNotification_ShouldInvokeFirstPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        FirstNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldInvokeSecondPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        SecondNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Publish_WhenNotification_ShouldExecutePipelinesInCorrectOrder()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(3);
        PipelineExecutionOrder[0].ShouldBe(nameof(FirstNotificationPipelineBehavior<TestNotification>));
        PipelineExecutionOrder[1].ShouldBe(nameof(SecondNotificationPipelineBehavior<TestNotification>));
        PipelineExecutionOrder[2].ShouldBe(nameof(TestNotificationHandler));
    }

    [Fact]
    public async Task Publish_WhenNotificationCalledMultipleTimes_ShouldIncrementAllInvocationCounts()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };

        // Act
        await _notificationDispatcher.Publish(notification);
        await _notificationDispatcher.Publish(notification);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(2);
        FirstNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(2);
        SecondNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(2);
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
    public async Task Publish_WithCancellationToken_ShouldInvokeHandlersAndPipelines()
    {
        // Arrange
        ResetAllCounters();
        var notification = new TestNotification { Data = "TestData" };
        var cancellationToken = new CancellationToken();

        // Act
        await _notificationDispatcher.Publish(notification, cancellationToken);

        // Assert
        TestNotificationHandler.InvocationCount.ShouldBe(1);
        FirstNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
        SecondNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(1);
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

        // Handlers and pipelines should not be invoked if token is canceled
        TestNotificationHandler.InvocationCount.ShouldBe(0);
        FirstNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(0);
        SecondNotificationPipelineBehavior<TestNotification>.InvocationCount.ShouldBe(0);
    }
}
