using Conduit.Behaviors;
using Conduit.Configurations;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Handlers;
using Conduit.Dispatchers;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Conduit;

/// <summary>
/// Provides extension methods for configuring Conduit services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ConduitExtensions
{
    /// <summary>
    /// Adds Conduit services, including request and notification dispatchers, handlers, and pipeline behaviors, to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which Conduit services will be added.</param>
    /// <param name="configAction">An action to configure the <see cref="ConduitServiceConfiguration"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConduit(this IServiceCollection services, Action<ConduitServiceConfiguration> configAction)
    {
        var configuration = new ConduitServiceConfiguration();
        configAction(configuration);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>));
        services.AddTransient(typeof(INotificationPipelineBehavior<>), typeof(NotificationExceptionProcessorBehavior<>));

        services.AddSingleton<IRequestDispatcher, RequestDispatcher>();
        services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();

        foreach (var assembly in configuration.AssembliesToRegister)
        {
            services.AddHandlersFromAssembly(assembly, configuration.HandlerLifetime);
        }

        foreach (var behavior in configuration.RequestPipelineBehaviors)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), behavior);
        }

        foreach (var behavior in configuration.NotificationPipelineBehaviors)
        {
            services.AddTransient(typeof(INotificationPipelineBehavior<>), behavior);
        }

        return services;
    }

    /// <summary>
    /// Registers all handlers (request and notification) from the specified assembly into the service collection.
    /// </summary>
    /// <param name="services">The service collection to which handlers will be added.</param>
    /// <param name="assembly">The assembly from which handlers will be registered.</param>
    /// <param name="serviceLifetime">The lifetime of the registered handlers.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(INotificationHandler<>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }
}
