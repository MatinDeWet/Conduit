using Conduit.Behaviors;
using Conduit.Contract.Requests;
using Shouldly;
using System.Reflection;

namespace Conduit.UnitTest.Tests.BehaviorTests;

public class NotificationExceptionProcessorBehaviorTests
{
    public class TestNotification : INotification
    {
        public string Data { get; set; } = null!;
    }

    private readonly NotificationExceptionProcessorBehavior<TestNotification> _behavior;

    public NotificationExceptionProcessorBehaviorTests()
    {
        _behavior = new NotificationExceptionProcessorBehavior<TestNotification>();
    }

    [Fact]
    public async Task Handle_WhenNoException_ShouldInvokeNext()
    {
        // Arrange
        var notification = new TestNotification { Data = "TestData" };
        var nextCalled = false;

        async Task Next()
        {
            nextCalled = true;
            await Task.CompletedTask;
        }

        // Act
        await _behavior.Handle(notification, Next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenTargetInvocationException_ShouldUnwrapAndThrowInnerException()
    {
        // Arrange
        var notification = new TestNotification { Data = "TestData" };
        var innerException = new InvalidOperationException("Inner exception");

        Task Next() => throw new TargetInvocationException(innerException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _behavior.Handle(notification, Next, CancellationToken.None));

        exception.ShouldBe(innerException);
    }

    [Fact]
    public async Task Handle_WhenAggregateExceptionWithSingleInnerException_ShouldUnwrapAndThrowInnerException()
    {
        // Arrange
        var notification = new TestNotification { Data = "TestData" };
        var innerException = new InvalidOperationException("Inner exception");

        Task Next() => throw new AggregateException(innerException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _behavior.Handle(notification, Next, CancellationToken.None));

        exception.ShouldBe(innerException);
    }

    [Fact]
    public async Task Handle_WhenAggregateExceptionWithMultipleInnerExceptions_ShouldThrowAggregateException()
    {
        // Arrange
        var notification = new TestNotification { Data = "TestData" };
        var innerException1 = new InvalidOperationException("Inner exception 1");
        var innerException2 = new InvalidOperationException("Inner exception 2");

        Task Next() => throw new AggregateException(innerException1, innerException2);

        // Act & Assert
        var exception = await Should.ThrowAsync<AggregateException>(
            async () => await _behavior.Handle(notification, Next, CancellationToken.None));

        exception.InnerExceptions.ShouldContain(innerException1);
        exception.InnerExceptions.ShouldContain(innerException2);
    }
}
