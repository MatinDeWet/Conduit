# Conduit

A lightweight .NET 10 library for in-process command, query, and domain-event handling.

Conduit gives you a small set of contracts for application-layer request handling and a simple registration model for wiring handlers through dependency injection.

## What It Provides

- `ICommand` and `ICommand<TResponse>` for write operations.
- `IQuery<TResponse>` for read operations.
- `ICommandManager<...>` and `IQueryManager<...>` handler contracts.
- `IDomainEvent` and `IDomainEventManager<TEvent>` for domain events.
- `IDomainEventsDispatcher` for queuing domain events for background processing.
- `AddCQRSSupport(...)` for assembly scanning and DI registration.

## Installation

```bash
dotnet add package MatinDeWet.Conduit
```

## Register Conduit

Register Conduit by pointing it at an assembly that contains your handlers.

```csharp
using Conduit;

services.AddCQRSSupport(typeof(Program));
```

You can also pass a type from your application layer instead of `Program`, as long as that assembly contains your handler implementations.

## Commands

Use commands for operations that change state.

Command without a response:

```csharp
using Ardalis.Result;
using Conduit.Contracts;

public sealed record CreateOrderCommand(string Number) : ICommand;

public sealed class CreateOrderCommandHandler : ICommandManager<CreateOrderCommand>
{
	public Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
	{
		// perform write operation
		return Task.FromResult(Result.Success());
	}
}
```

Command with a response:

```csharp
using Ardalis.Result;
using Conduit.Contracts;

public sealed record CreateOrderCommand(string Number) : ICommand<Guid>;

public sealed class CreateOrderCommandHandler : ICommandManager<CreateOrderCommand, Guid>
{
	public Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
	{
		Guid orderId = Guid.NewGuid();
		return Task.FromResult(Result.Success(orderId));
	}
}
```

## Queries

Use queries for read operations.

```csharp
using Ardalis.Result;
using Conduit.Contracts;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;

public sealed class GetOrderQueryHandler : IQueryManager<GetOrderQuery, OrderDto>
{
	public Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
	{
		OrderDto order = new(request.OrderId, "SO-1001");
		return Task.FromResult(Result.Success(order));
	}
}

public sealed record OrderDto(Guid Id, string Number);
```

## Domain Events

Domain events are marker types that implement `IDomainEvent` from `Conduit.Domain.Contracts`.

```csharp
using Conduit.Domain.Contracts;

public sealed record OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent;
```

Handlers implement `IDomainEventManager<TEvent>`.

```csharp
using Conduit.Contracts;

public sealed class OrderCreatedDomainEventHandler : IDomainEventManager<OrderCreatedDomainEvent>
{
	public Task Handle(OrderCreatedDomainEvent request, CancellationToken cancellationToken)
	{
		// react to event
		return Task.CompletedTask;
	}
}
```

Dispatch events through `IDomainEventsDispatcher`.

```csharp
using Conduit.Contracts;

public sealed class OrderService(IDomainEventsDispatcher domainEventsDispatcher)
{
	public async Task PublishAsync(Guid orderId, CancellationToken cancellationToken)
	{
		await domainEventsDispatcher.DispatchAsync(
			[new OrderCreatedDomainEvent(orderId)],
			cancellationToken);
	}
}
```

Conduit queues domain events and processes them in the background through its hosted service registration.

## Notes

- Handler implementations are discovered by assembly scanning.
- Commands and queries return `Ardalis.Result` or `Ardalis.Result<T>`.
- Domain events are dispatched asynchronously through the built-in queue and background processor.
- `TryDecorateIfImplementationsExist(...)` is available when you want to apply decorators only if matching handlers are registered.

