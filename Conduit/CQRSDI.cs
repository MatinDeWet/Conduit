using Conduit.Contracts;
using Conduit.Implementation;
using Conduit.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit;

/// <summary>
/// Dependency injection registration helpers for Conduit command, query, and domain event handlers.
/// </summary>
public static class CQRSDI
{
    /// <summary>
    /// Registers Conduit handlers from the assembly containing <paramref name="assemplyPointer"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemplyPointer">A type located in the target assembly.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCQRSSupport(this IServiceCollection services, Type assemplyPointer)
    {
        return services.AddCQRSSupport(assemplyPointer, null);
    }

    /// <summary>
    /// Registers Conduit handlers from the assembly containing <paramref name="assemplyPointer"/> and allows optional decorator configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemplyPointer">A type located in the target assembly.</param>
    /// <param name="configureDecorators">Optional decorator registration logic.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCQRSSupport(this IServiceCollection services, Type assemplyPointer, Action<IServiceCollection>? configureDecorators)
    {
        services.Scan(scan => scan.FromAssembliesOf(assemplyPointer)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryManager<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandManager<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandManager<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Apply additional decorators if provided
        configureDecorators?.Invoke(services);

        services.Scan(scan => scan.FromAssembliesOf(assemplyPointer)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventManager<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddSingleton<IBackgroundDomainEventQueue, BackgroundDomainEventQueue>();
        services.AddHostedService<BackgroundDomainEventProcessor>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    /// <summary>
    /// Safely decorates a service type only if implementations are registered for that type.
    /// This prevents DI container errors when no implementations exist.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="serviceType">The service type to decorate (generic type definition)</param>
    /// <param name="decoratorType">The decorator type (generic type definition)</param>
    public static void TryDecorateIfImplementationsExist(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        // Check if there are any registered implementations for the service type
        bool hasImplementations = services.Any(descriptor =>
            descriptor.ServiceType.IsGenericType &&
            descriptor.ServiceType.GetGenericTypeDefinition() == serviceType);

        if (hasImplementations)
        {
            services.Decorate(serviceType, decoratorType);
        }
    }
}
