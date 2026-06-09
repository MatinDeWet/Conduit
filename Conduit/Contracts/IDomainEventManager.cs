using Conduit.Domain.Contracts;

namespace Conduit.Contracts;

/// <summary>
/// Handles a specific domain event type.
/// </summary>
/// <typeparam name="T">The domain event type.</typeparam>
public interface IDomainEventManager<in T> where T : IDomainEvent
{
    /// <summary>
    /// Handles a domain event instance.
    /// </summary>
    /// <param name="request">The domain event to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when processing has finished.</returns>
    Task Handle(T request, CancellationToken cancellationToken);
}
