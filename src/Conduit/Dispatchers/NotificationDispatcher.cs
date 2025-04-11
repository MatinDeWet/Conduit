using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Dispatchers;

public class NotificationDispatcher(IServiceProvider _serviceProvider) : INotificationDispatcher
{
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var notificationType = notification.GetType();

        var behaviors = _serviceProvider.GetServices(
            typeof(INotificationPipelineBehavior<>).MakeGenericType(notificationType))
            .OfType<INotificationPipelineBehavior<TNotification>>()
            .ToList();

        NotificationHandlerDelegate pipeline = () => PublishToHandlers(notification, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var nextDelegate = pipeline;
            pipeline = () => currentBehavior.Handle(notification, nextDelegate, cancellationToken);
        }

        await pipeline();
    }

    private async Task PublishToHandlers<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var handlers = _serviceProvider.GetServices(handlerType)
            ?? throw new InvalidOperationException($"No handlers found for notification type {notificationType.Name}.");

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("Handle")
                ?? throw new InvalidOperationException($"Handle method not found on handler for {notificationType.Name}.");

            var task = method.Invoke(handler, [notification, cancellationToken]) as Task
                ?? throw new InvalidOperationException($"Handler for {notificationType.Name} did not return a Task.");

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}
