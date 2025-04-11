using BenchmarkDotNet.Attributes;
using Conduit.Behaviors;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Conduit.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class NotificationBenchmarks
{
    private INotificationDispatcher _notificationDispatcherWithoutPipelines = null!;
    private INotificationDispatcher _notificationDispatcherWithOnePipeline = null!;
    private INotificationDispatcher _notificationDispatcherWithTwoPipelines = null!;

    private SimpleNotification _smallNotification = null!;
    private SimpleNotification _mediumNotification = null!;
    private SimpleNotification _largeNotification = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup dispatchers with different pipeline configurations
        _notificationDispatcherWithoutPipelines = CreateDispatcher(0);
        _notificationDispatcherWithOnePipeline = CreateDispatcher(1);
        _notificationDispatcherWithTwoPipelines = CreateDispatcher(2);

        // Create notifications with different payload sizes
        _smallNotification = new SimpleNotification { Data = CreateString(100) };
        _mediumNotification = new SimpleNotification { Data = CreateString(10_000) };
        _largeNotification = new SimpleNotification { Data = CreateString(1_000_000) };
    }

    private static string CreateString(int size)
    {
        var builder = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            builder.Append((char)('A' + (i % 26)));
        }
        return builder.ToString();
    }

    private static INotificationDispatcher CreateDispatcher(int pipelineCount)
    {
        var services = new ServiceCollection();

        // Add exception processor as the first pipeline
        services.AddTransient(typeof(INotificationPipelineBehavior<>), typeof(NotificationExceptionProcessorBehavior<>));

        services.AddConduit(config =>
        {
            // Register handlers
            config.RegisterServicesFromAssemblyContaining<SimpleNotificationHandler>();

            // Add requested number of additional pipelines
            if (pipelineCount >= 1)
            {
                config.AddNotificationBehavior(typeof(LoggingNotificationPipelineBehavior<>));
            }

            if (pipelineCount >= 2)
            {
                config.AddNotificationBehavior(typeof(ValidationNotificationPipelineBehavior<>));
            }
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<INotificationDispatcher>();
    }

    #region Single Handler

    [Benchmark]
    public async Task SimpleNotification_Small_NoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithoutPipelines.Publish(_smallNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Small_OnePipeline_SingleHandler()
    {
        await _notificationDispatcherWithOnePipeline.Publish(_smallNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Small_TwoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithTwoPipelines.Publish(_smallNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Medium_NoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithoutPipelines.Publish(_mediumNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Medium_OnePipeline_SingleHandler()
    {
        await _notificationDispatcherWithOnePipeline.Publish(_mediumNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Medium_TwoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithTwoPipelines.Publish(_mediumNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Large_NoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithoutPipelines.Publish(_largeNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Large_OnePipeline_SingleHandler()
    {
        await _notificationDispatcherWithOnePipeline.Publish(_largeNotification);
    }

    [Benchmark]
    public async Task SimpleNotification_Large_TwoPipelines_SingleHandler()
    {
        await _notificationDispatcherWithTwoPipelines.Publish(_largeNotification);
    }

    #endregion

    #region Support Classes

    public class SimpleNotification : INotification
    {
        public string Data { get; set; } = null!;
    }

    public class SimpleNotificationHandler : INotificationHandler<SimpleNotification>
    {
        public Task Handle(SimpleNotification notification, CancellationToken cancellationToken)
        {
            // Simulate minimal processing - we want to benchmark pipeline overhead
            _ = notification.Data.Length;
            return Task.CompletedTask;
        }
    }

    // Second handler to benchmark multiple handlers
    public class SecondSimpleNotificationHandler : INotificationHandler<SimpleNotification>
    {
        public Task Handle(SimpleNotification notification, CancellationToken cancellationToken)
        {
            // Simulate minimal processing - we want to benchmark pipeline overhead
            _ = notification.Data.Length;
            return Task.CompletedTask;
        }
    }

    public class LoggingNotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            // Simulate logging operation overhead
            var notificationType = notification.GetType().Name;
            _ = $"Processing notification {notificationType}";

            return next();
        }
    }

    public class ValidationNotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            // Simulate validation overhead
            if (notification is SimpleNotification simpleNotification)
            {
                _ = !string.IsNullOrEmpty(simpleNotification.Data);
            }

            return next();
        }
    }

    #endregion
}
