using System.Threading.Channels;
using Conduit.Domain.Contracts;

namespace Conduit.Services;

internal sealed class BackgroundDomainEventQueue : IBackgroundDomainEventQueue
{
    private readonly Channel<IDomainEvent> _channel;

    public BackgroundDomainEventQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<IDomainEvent>(options);
    }

    public async ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));
        await _channel.Writer.WriteAsync(domainEvent, cancellationToken);
    }

    public IAsyncEnumerable<IDomainEvent> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

/// <summary>
/// Contract for queuing and streaming domain events in the background processor.
/// </summary>
public interface IBackgroundDomainEventQueue
{
    /// <summary>
    /// Enqueues a domain event for background processing.
    /// </summary>
    /// <param name="domainEvent">The domain event to enqueue.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task that completes when the event is queued.</returns>
    ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues domain events as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous stream of domain events.</returns>
    IAsyncEnumerable<IDomainEvent> DequeueAsync(CancellationToken cancellationToken = default);
}
