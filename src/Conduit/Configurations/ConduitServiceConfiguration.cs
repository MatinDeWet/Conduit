using Conduit.Contract.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Conduit.Configurations;

public class ConduitServiceConfiguration
{
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Transient;

    internal List<Assembly> AssembliesToRegister { get; } = [];

    internal List<Type> RequestPipelineBehaviors { get; } = [];

    internal List<Type> NotificationPipelineBehaviors { get; } = [];

    public ConduitServiceConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);
        return this;
    }

    public ConduitServiceConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        return RegisterServicesFromAssemblies(typeof(T).Assembly);
    }

    public ConduitServiceConfiguration RegisterServicesFromAssemblyContaining(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return RegisterServicesFromAssemblies(type.Assembly);
    }

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
