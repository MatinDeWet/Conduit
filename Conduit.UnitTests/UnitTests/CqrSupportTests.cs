using Ardalis.Result;
using Conduit.Contracts;
using Conduit.Domain.Contracts;
using Conduit.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTests.UnitTests;

public sealed class CqrSupportTests
{
    [Fact]
    public void AddCqrSupport_ShouldRegisterHandlersAndInfrastructure()
    {
        var services = new ServiceCollection();

        services.AddCQRSSupport(typeof(PingCommandHandler));
        bool hasHostedRegistration = services.Any(descriptor => descriptor.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService));
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<ICommandManager<PingCommand>>().ShouldNotBeNull();
        provider.GetService<ICommandManager<PingWithResponseCommand, string>>().ShouldNotBeNull();
        provider.GetService<IQueryManager<GetValueQuery, int>>().ShouldNotBeNull();
        provider.GetService<IDomainEventManager<SampleDomainEvent>>().ShouldNotBeNull();

        provider.GetService<IDomainEventsDispatcher>().ShouldNotBeNull();
        provider.GetService<IBackgroundDomainEventQueue>().ShouldNotBeNull();
        hasHostedRegistration.ShouldBeTrue();
    }

    [Fact]
    public async Task DomainEventsDispatcher_ShouldEnqueueEventsInOrder()
    {
        var services = new ServiceCollection();
        services.AddCQRSSupport(typeof(PingCommandHandler));

        using ServiceProvider provider = services.BuildServiceProvider();
        IDomainEventsDispatcher dispatcher = provider.GetRequiredService<IDomainEventsDispatcher>();
        IBackgroundDomainEventQueue queue = provider.GetRequiredService<IBackgroundDomainEventQueue>();

        var expectedEvents = new List<SampleDomainEvent>
        {
            new(Guid.NewGuid()),
            new(Guid.NewGuid())
        };

        await dispatcher.DispatchAsync(expectedEvents, CancellationToken.None);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        IAsyncEnumerator<IDomainEvent> enumerator = queue.DequeueAsync(cts.Token).GetAsyncEnumerator(cts.Token);

        var actualEvents = new List<SampleDomainEvent>();

        try
        {
            while (actualEvents.Count < expectedEvents.Count)
            {
                (await enumerator.MoveNextAsync()).ShouldBeTrue();
                enumerator.Current.ShouldBeOfType<SampleDomainEvent>();
                actualEvents.Add((SampleDomainEvent)enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        actualEvents.Select(x => x.EventId).ShouldBe(expectedEvents.Select(x => x.EventId));
    }

    [Fact]
    public void TryDecorateIfImplementationsExist_ShouldNotThrow_WhenNoMatchingRegistrationsExist()
    {
        var services = new ServiceCollection();

        Should.NotThrow(() =>
            services.TryDecorateIfImplementationsExist(
                typeof(IQueryManager<,>),
                typeof(QueryManagerDecorator<,>)));
    }

    [Fact]
    public void TryDecorateIfImplementationsExist_ShouldDecorate_WhenMatchingRegistrationsExist()
    {
        var services = new ServiceCollection();

        services.AddScoped<IQueryManager<GetValueQuery, int>, GetValueQueryHandler>();
        services.TryDecorateIfImplementationsExist(typeof(IQueryManager<,>), typeof(QueryManagerDecorator<,>));

        using ServiceProvider provider = services.BuildServiceProvider();
        IQueryManager<GetValueQuery, int> resolved = provider.GetRequiredService<IQueryManager<GetValueQuery, int>>();

        resolved.ShouldBeOfType<QueryManagerDecorator<GetValueQuery, int>>();
    }

    private sealed record PingCommand : ICommand;

    private sealed record PingWithResponseCommand(string Message) : ICommand<string>;

    private sealed record GetValueQuery : IQuery<int>;

    private sealed record SampleDomainEvent(Guid EventId) : IDomainEvent;

    private sealed class PingCommandHandler : ICommandManager<PingCommand>
    {
        public Task<Result> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class PingWithResponseCommandHandler : ICommandManager<PingWithResponseCommand, string>
    {
        public Task<Result<string>> Handle(PingWithResponseCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Message));
        }
    }

    private sealed class GetValueQueryHandler : IQueryManager<GetValueQuery, int>
    {
        public Task<Result<int>> Handle(GetValueQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(42));
        }
    }

    private sealed class SampleDomainEventHandler : IDomainEventManager<SampleDomainEvent>
    {
        public Task Handle(SampleDomainEvent request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class QueryManagerDecorator<TQuery, TResponse>(
        IQueryManager<TQuery, TResponse> inner) : IQueryManager<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            return inner.Handle(request, cancellationToken);
        }
    }
}
