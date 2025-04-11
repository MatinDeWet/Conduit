using Conduit.Contract.Requests;

namespace Conduit.Dispatchers.Contracts;

public interface INotificationDispatcher
{
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
