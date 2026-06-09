# Conduit

A lightweight .NET 10 library for in-process command, query, and domain-event flows.

## Features

- Clear contracts for commands, queries, and domain events.
- Assembly scanning registration helpers for handlers.
- Background domain-event queue and processor.
- Small, focused API surface designed for application-layer orchestration.

## Installation

```bash
dotnet add package MatinDeWet.Conduit
```

## Quick Start

```csharp
using Conduit;

services.AddCQRSSupport(typeof(Program));
```

Implement handlers by targeting the Conduit contracts:

- `ICommandManager<TCommand>`
- `ICommandManager<TCommand, TResponse>`
- `IQueryManager<TQuery, TResponse>`
- `IDomainEventManager<TDomainEvent>`

Domain events should implement `IDomainEvent` from `Conduit.Domain.Contracts`.

## Build

```bash
dotnet build Conduit.slnx
```

## Pack

```bash
dotnet pack Conduit/Conduit.csproj -c Release -o ./nupkgs
```
