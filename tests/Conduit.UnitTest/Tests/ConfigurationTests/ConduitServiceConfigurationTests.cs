using Conduit.Configurations;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.ConfigurationTests;

public class ConduitServiceConfigurationTests
{
    #region Constructor and Default Values

    [Fact]
    public void Constructor_ShouldSetDefaultHandlerLifetime()
    {
        // Act
        var config = new ConduitServiceConfiguration();

        // Assert
        config.HandlerLifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyCollections()
    {
        // Act
        var config = new ConduitServiceConfiguration();

        // Assert
        config.AssembliesToRegister.ShouldNotBeNull();
        config.AssembliesToRegister.ShouldBeEmpty();
        config.RequestPipelineBehaviors.ShouldNotBeNull();
        config.RequestPipelineBehaviors.ShouldBeEmpty();
        config.NotificationPipelineBehaviors.ShouldNotBeNull();
        config.NotificationPipelineBehaviors.ShouldBeEmpty();
    }

    #endregion

    #region RegisterServicesFromAssemblies Tests

    [Fact]
    public void RegisterServicesFromAssemblies_WhenCalled_ShouldAddAssembliesToList()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var assembly1 = typeof(string).Assembly;
        var assembly2 = typeof(ConduitServiceConfiguration).Assembly;

        // Act
        var result = config.RegisterServicesFromAssemblies(assembly1, assembly2);

        // Assert
        config.AssembliesToRegister.ShouldContain(assembly1);
        config.AssembliesToRegister.ShouldContain(assembly2);
        config.AssembliesToRegister.Count.ShouldBe(2);
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    [Fact]
    public void RegisterServicesFromAssemblies_WhenCalledMultipleTimes_ShouldAddAllAssemblies()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var assembly1 = typeof(string).Assembly;
        var assembly2 = typeof(ConduitServiceConfiguration).Assembly;

        // Act
        config.RegisterServicesFromAssemblies(assembly1);
        config.RegisterServicesFromAssemblies(assembly2);

        // Assert
        config.AssembliesToRegister.ShouldContain(assembly1);
        config.AssembliesToRegister.ShouldContain(assembly2);
        config.AssembliesToRegister.Count.ShouldBe(2);
    }

    [Fact]
    public void RegisterServicesFromAssemblies_WhenCalledWithEmptyArray_ShouldNotAddAnything()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();

        // Act
        var result = config.RegisterServicesFromAssemblies();

        // Assert
        config.AssembliesToRegister.ShouldBeEmpty();
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    #endregion

    #region RegisterServicesFromAssemblyContaining<T> Tests

    [Fact]
    public void RegisterServicesFromAssemblyContaining_Generic_ShouldAddAssemblyToList()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var expectedAssembly = typeof(ConduitServiceConfiguration).Assembly;

        // Act
        var result = config.RegisterServicesFromAssemblyContaining<ConduitServiceConfiguration>();

        // Assert
        config.AssembliesToRegister.ShouldContain(expectedAssembly);
        config.AssembliesToRegister.Count.ShouldBe(1);
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    #endregion

    #region RegisterServicesFromAssemblyContaining(Type) Tests

    [Fact]
    public void RegisterServicesFromAssemblyContaining_Type_ShouldAddAssemblyToList()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var type = typeof(ConduitServiceConfiguration);
        var expectedAssembly = type.Assembly;

        // Act
        var result = config.RegisterServicesFromAssemblyContaining(type);

        // Assert
        config.AssembliesToRegister.ShouldContain(expectedAssembly);
        config.AssembliesToRegister.Count.ShouldBe(1);
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    [Fact]
    public void RegisterServicesFromAssemblyContaining_WhenTypeIsNull_ShouldThrowException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        Type nullType = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.RegisterServicesFromAssemblyContaining(nullType));
    }

    #endregion

    #region AddRequestBehavior Tests

    // Define a valid implementation of IPipelineBehavior for testing
    public class ValidRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return next();
        }
    }

    // Define an invalid non-generic class that doesn't implement IPipelineBehavior
    public class NonGenericClass { }

    // Define an invalid generic class that doesn't implement IPipelineBehavior
    public class InvalidGenericBehavior<T> { }

    [Fact]
    public void AddRequestBehavior_WhenValidBehavior_ShouldAddToList()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var behaviorType = typeof(ValidRequestBehavior<,>);

        // Act
        var result = config.AddRequestBehavior(behaviorType);

        // Assert
        config.RequestPipelineBehaviors.ShouldContain(behaviorType);
        config.RequestPipelineBehaviors.Count.ShouldBe(1);
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    [Fact]
    public void AddRequestBehavior_WhenNonGenericType_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var nonGenericType = typeof(NonGenericClass);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.AddRequestBehavior(nonGenericType));
        exception.Message.ShouldContain("open generic type");
        exception.ParamName.ShouldBe("openBehaviorType");
    }

    [Fact]
    public void AddRequestBehavior_WhenGenericTypeNotImplementingInterface_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var invalidType = typeof(InvalidGenericBehavior<>);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.AddRequestBehavior(invalidType));
        exception.Message.ShouldContain("IPipelineBehavior");
        exception.ParamName.ShouldBe("openBehaviorType");
    }

    [Fact]
    public void AddRequestBehavior_WhenTypeIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddRequestBehavior(null!));
    }

    #endregion

    #region AddNotificationBehavior Tests

    // Define a valid implementation of INotificationPipelineBehavior for testing
    public class ValidNotificationBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        public Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
        {
            return next();
        }
    }

    [Fact]
    public void AddNotificationBehavior_WhenValidBehavior_ShouldAddToList()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var behaviorType = typeof(ValidNotificationBehavior<>);

        // Act
        var result = config.AddNotificationBehavior(behaviorType);

        // Assert
        config.NotificationPipelineBehaviors.ShouldContain(behaviorType);
        config.NotificationPipelineBehaviors.Count.ShouldBe(1);
        result.ShouldBeSameAs(config); // Verify fluent interface
    }

    [Fact]
    public void AddNotificationBehavior_WhenNonGenericType_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var nonGenericType = typeof(NonGenericClass);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.AddNotificationBehavior(nonGenericType));
        exception.Message.ShouldContain("open generic type");
        exception.ParamName.ShouldBe("openBehaviorType");
    }

    [Fact]
    public void AddNotificationBehavior_WhenGenericTypeNotImplementingInterface_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();
        var invalidType = typeof(InvalidGenericBehavior<>);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.AddNotificationBehavior(invalidType));
        exception.Message.ShouldContain("INotificationPipelineBehavior");
        exception.ParamName.ShouldBe("openBehaviorType");
    }

    [Fact]
    public void AddNotificationBehavior_WhenTypeIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new ConduitServiceConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddNotificationBehavior(null!));
    }

    #endregion
}