# Conduit

[![NuGet Version](https://img.shields.io/nuget/v/MatinDeWet.Conduit)](https://www.nuget.org/packages/MatinDeWet.Conduit/) 
[![NuGet Version](https://img.shields.io/nuget/v/MatinDeWet.Conduit.Contract)](https://www.nuget.org/packages/MatinDeWet.Conduit.Contract/) 
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/MatinDeWet/Conduit/ci.yml)](https://github.com/MatinDeWet/Conduit)

A mediator pattern implementation for .NET applications.

## Overview

Conduit is a flexible mediator library designed to simplify the implementation of the Command Query Responsibility Segregation (CQRS) pattern in .NET applications. It provides a clean and structured approach to handling requests, commands, and notifications, allowing you to focus on your business logic rather than the infrastructure.

## Features

- **Simple, intuitive API**: Easy to understand and implement in your projects
- **Full CQRS support**: Handle commands, queries, and notifications with dedicated handlers
- **Powerful pipeline behaviors**: Implement cross-cutting concerns like validation, logging, and exception handling
- **Dependency injection friendly**: Seamlessly integrates with Microsoft's dependency injection
- **High performance**: Designed with performance in mind
- **Fully async/await compatible**: Support for asynchronous operations