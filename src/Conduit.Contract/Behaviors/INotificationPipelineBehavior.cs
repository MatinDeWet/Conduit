using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;

namespace Conduit.Contract.Behaviors;

public interface INotificationPipelineBehavior<in TNotification>
    where TNotification : INotification
{
    Task Handle(
        TNotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken);
}
