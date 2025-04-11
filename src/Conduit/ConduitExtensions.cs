using Conduit.Behaviors;
using Conduit.Configurations;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Handlers;
using Conduit.Dispatchers;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Conduit;

public static class ConduitExtensions
{
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
            //services.AddPipelineBehaviorsFromAssembly(assembly, configuration.PipelineLifetime);
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

    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime)
    {
        // Register request handlers without response
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        // Register request handlers with response
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        // Register notification handlers
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(INotificationHandler<>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }

    //public static IServiceCollection AddPipelineBehaviorsFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime)
    //{
    //    // Register request pipeline behaviors
    //    services.Scan(scan => scan
    //        .FromAssemblies(assembly)
    //        .AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)))
    //        .AsImplementedInterfaces()
    //        .WithLifetime(serviceLifetime));

    //    // Register notification pipeline behaviors
    //    services.Scan(scan => scan
    //        .FromAssemblies(assembly)
    //        .AddClasses(classes => classes.AssignableTo(typeof(INotificationPipelineBehavior<>)))
    //        .AsImplementedInterfaces()
    //        .WithLifetime(serviceLifetime));

    //    return services;
    //}
}
