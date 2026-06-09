using Conduit.Contracts;
using Conduit.Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conduit.Services;

internal sealed class BackgroundDomainEventProcessor(
    IBackgroundDomainEventQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundDomainEventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background Domain Event Processor started");

        await foreach (IDomainEvent domainEvent in queue.DequeueAsync(stoppingToken))
        {
            try
            {
                await ProcessDomainEventAsync(domainEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing domain event {EventType}", domainEvent.GetType().Name);
            }
        }

        logger.LogInformation("Background Domain Event Processor stopped");
    }

    private async Task ProcessDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        Type domainEventType = domainEvent.GetType();
        Type handlerType = typeof(IDomainEventManager<>).MakeGenericType(domainEventType);

        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            try
            {
                await InvokeHandlerAsync(handler, domainEvent, domainEventType, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler {HandlerType} failed for event {EventType}",
                    handler.GetType().Name, domainEventType.Name);
            }
        }
    }

    private static async Task InvokeHandlerAsync(
        object handler,
        IDomainEvent domainEvent,
        Type domainEventType,
        CancellationToken cancellationToken)
    {
        Type handlerWrapperType = typeof(HandlerWrapper<>).MakeGenericType(domainEventType);
        dynamic handlerWrapper = Activator.CreateInstance(handlerWrapperType, handler)!;
        await handlerWrapper.HandleAsync((dynamic)domainEvent, cancellationToken);
    }

    private sealed class HandlerWrapper<T>(IDomainEventManager<T> handler) where T : IDomainEvent
    {
        public async Task HandleAsync(T domainEvent, CancellationToken cancellationToken)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }
}
