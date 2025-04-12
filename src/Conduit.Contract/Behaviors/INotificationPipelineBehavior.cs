using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;

namespace Conduit.Contract.Behaviors;

/// <summary>
/// Defines a pipeline behavior for processing notifications before they reach their handlers.
/// </summary>
/// <typeparam name="TNotification">The type of notification being processed.</typeparam>
public interface INotificationPipelineBehavior<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the notification by applying custom behavior and invoking the next delegate in the pipeline.
    /// </summary>
    /// <param name="notification">The notification being processed.</param>
    /// <param name="next">The next delegate in the pipeline to invoke.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(
        TNotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken);
}
