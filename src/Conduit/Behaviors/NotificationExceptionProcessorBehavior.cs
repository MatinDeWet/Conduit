using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Conduit.Behaviors;

/// <summary>
/// A pipeline behavior that processes exceptions thrown during notification handling.
/// Unwraps common exception types to expose the original exception details.
/// </summary>
/// <typeparam name="TNotification">The type of notification being processed</typeparam>
public class NotificationExceptionProcessorBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Executes the next delegate in the notification pipeline and handles specific exception cases.
    /// Unwraps TargetInvocationException and AggregateException with single inner exceptions
    /// to simplify debugging and error handling.
    /// </summary>
    /// <param name="notification">The notification being processed</param>
    /// <param name="next">The next handler delegate in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
            throw;
        }
    }
}
