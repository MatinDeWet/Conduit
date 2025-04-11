using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Conduit.Behaviors;

public class NotificationExceptionProcessorBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    public async Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap reflection exceptions
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // Never reached, but compiler needs it
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            // Unwrap single inner exception from AggregateException
            ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
            throw; // Never reached, but compiler needs it
        }
    }
}
