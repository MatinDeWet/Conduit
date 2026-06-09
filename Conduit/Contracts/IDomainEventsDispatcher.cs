using Conduit.Domain.Contracts;

namespace Conduit.Contracts;

/// <summary>
/// Dispatches domain events for asynchronous processing.
/// </summary>
public interface IDomainEventsDispatcher
{
    /// <summary>
    /// Dispatches one or more domain events.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when all events have been enqueued.</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
