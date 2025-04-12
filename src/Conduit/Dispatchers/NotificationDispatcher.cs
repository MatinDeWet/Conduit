using Conduit.Constants;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Dispatchers;

/// <summary>
/// Dispatches notifications to their respective handlers and applies notification pipeline behaviors.
/// </summary>
public class NotificationDispatcher(IServiceProvider _serviceProvider) : INotificationDispatcher
{
    /// <summary>
    /// Publishes a notification to all registered handlers, applying any configured pipeline behaviors.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being published.</typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the notification is null.</exception>
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

    /// <summary>
    /// Publishes the notification directly to all registered handlers without applying pipeline behaviors.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being published.</typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handlers are found for the notification type or if a handler does not return a Task.
    /// </exception>
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
            var method = handlerType.GetMethod(ConduitConstants.Handle)
                ?? throw new InvalidOperationException($"Handle method not found on handler for {notificationType.Name}.");

            var task = method.Invoke(handler, [notification, cancellationToken]) as Task
                ?? throw new InvalidOperationException($"Handler for {notificationType.Name} did not return a Task.");

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}
