using Conduit.Contract.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Conduit.Configurations;

/// <summary>
/// Configuration class for setting up Conduit services, including request and notification pipeline behaviors.
/// </summary>
public class ConduitServiceConfiguration
{
    /// <summary>
    /// Gets or sets the lifetime of the handlers registered in the service container.
    /// Default is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets the list of assemblies from which services will be registered.
    /// </summary>
    internal List<Assembly> AssembliesToRegister { get; } = [];

    /// <summary>
    /// Gets the list of request pipeline behaviors to be registered.
    /// </summary>
    internal List<Type> RequestPipelineBehaviors { get; } = [];

    /// <summary>
    /// Gets the list of notification pipeline behaviors to be registered.
    /// </summary>
    internal List<Type> NotificationPipelineBehaviors { get; } = [];

    /// <summary>
    /// Registers services from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to register services from.</param>
    /// <returns>The current <see cref="ConduitServiceConfiguration"/> instance.</returns>
    public ConduitServiceConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Registers services from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly will be registered.</typeparam>
    /// <returns>The current <see cref="ConduitServiceConfiguration"/> instance.</returns>
    public ConduitServiceConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        return RegisterServicesFromAssemblies(typeof(T).Assembly);
    }

    /// <summary>
    /// Registers services from the assembly containing the specified type.
    /// </summary>
    /// <param name="type">The type whose assembly will be registered.</param>
    /// <returns>The current <see cref="ConduitServiceConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided type is null.</exception>
    public ConduitServiceConfiguration RegisterServicesFromAssemblyContaining(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return RegisterServicesFromAssemblies(type.Assembly);
    }

    /// <summary>
    /// Adds a request pipeline behavior to the configuration.
    /// </summary>
    /// <param name="openBehaviorType">The open generic type of the behavior to add.</param>
    /// <returns>The current <see cref="ConduitServiceConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided type is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the type is not an open generic type or does not implement <see cref="IPipelineBehavior{TRequest, TResponse}"/>.</exception>
    public ConduitServiceConfiguration AddRequestBehavior(Type openBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openBehaviorType, nameof(openBehaviorType));

        if (!openBehaviorType.IsGenericType)
        {
            throw new ArgumentException("The type must be an open generic type.", nameof(openBehaviorType));
        }

        if (!openBehaviorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
        {
            throw new ArgumentException(
                $"The type must implement {typeof(IPipelineBehavior<,>).Name}.",
                nameof(openBehaviorType));
        }

        RequestPipelineBehaviors.Add(openBehaviorType);
        return this;
    }

    /// <summary>
    /// Adds a notification pipeline behavior to the configuration.
    /// </summary>
    /// <param name="openBehaviorType">The open generic type of the behavior to add.</param>
    /// <returns>The current <see cref="ConduitServiceConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided type is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the type is not an open generic type or does not implement <see cref="INotificationPipelineBehavior{TNotification}"/>.</exception>
    public ConduitServiceConfiguration AddNotificationBehavior(Type openBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openBehaviorType, nameof(openBehaviorType));

        if (!openBehaviorType.IsGenericType)
        {
            throw new ArgumentException("The type must be an open generic type.", nameof(openBehaviorType));
        }

        if (!openBehaviorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationPipelineBehavior<>)))
        {
            throw new ArgumentException(
                $"The type must implement {typeof(INotificationPipelineBehavior<>).Name}.",
                nameof(openBehaviorType));
        }

        NotificationPipelineBehaviors.Add(openBehaviorType);
        return this;
    }
}
