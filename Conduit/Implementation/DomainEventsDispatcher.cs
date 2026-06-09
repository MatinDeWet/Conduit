using Conduit.Contracts;
using Conduit.Domain.Contracts;
using Conduit.Services;

namespace Conduit.Implementation;

internal sealed class DomainEventsDispatcher(IBackgroundDomainEventQueue eventQueue) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await eventQueue.EnqueueAsync(domainEvent, cancellationToken);
        }
    }
}
