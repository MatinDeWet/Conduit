using Conduit.Contract.Requests;

namespace Conduit.Contract.Handlers;

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}