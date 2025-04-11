using Conduit.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using Shouldly;
using System.Reflection;

namespace Conduit.UnitTest.Tests.BehaviorTests;

public class RequestExceptionProcessorBehaviorTests
{
    public class TestRequest : IBaseRequest { }

    private readonly RequestExceptionProcessorBehavior<TestRequest, string> _behavior;

    public RequestExceptionProcessorBehaviorTests()
    {
        _behavior = new RequestExceptionProcessorBehavior<TestRequest, string>();
    }

    [Fact]
    public async Task Handle_WhenNoException_ShouldInvokeNext()
    {
        // Arrange
        var request = new TestRequest();
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }

        // Act
        var result = await _behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.ShouldBe("Success");
    }

    [Fact]
    public async Task Handle_WhenTargetInvocationException_ShouldUnwrapAndThrowInnerException()
    {
        // Arrange
        var request = new TestRequest();
        var innerException = new InvalidOperationException("Inner exception");

        Task<string> Next() => throw new TargetInvocationException(innerException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _behavior.Handle(request, Next, CancellationToken.None));

        exception.ShouldBe(innerException);
    }

    [Fact]
    public async Task Handle_WhenAggregateExceptionWithSingleInnerException_ShouldUnwrapAndThrowInnerException()
    {
        // Arrange
        var request = new TestRequest();
        var innerException = new InvalidOperationException("Inner exception");

        Task<string> Next() => throw new AggregateException(innerException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _behavior.Handle(request, Next, CancellationToken.None));

        exception.ShouldBe(innerException);
    }

    [Fact]
    public async Task Handle_WhenAggregateExceptionWithMultipleInnerExceptions_ShouldThrowAggregateException()
    {
        // Arrange
        var request = new TestRequest();
        var innerException1 = new InvalidOperationException("Inner exception 1");
        var innerException2 = new InvalidOperationException("Inner exception 2");

        Task<string> Next() => throw new AggregateException(innerException1, innerException2);

        // Act & Assert
        var exception = await Should.ThrowAsync<AggregateException>(
            async () => await _behavior.Handle(request, Next, CancellationToken.None));

        exception.InnerExceptions.ShouldContain(innerException1);
        exception.InnerExceptions.ShouldContain(innerException2);
    }
}
