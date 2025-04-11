using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Models;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.RequestTests;

public class RequestWithMultiplePipelinesTests
{
    public static readonly List<string> PipelineExecutionOrder = [];

    public class RequestWithHanlderWithoutResponseRequest : IRequest
    {
        public string Data { get; set; } = null!;
    }

    public class RequestWithHandlerWithResponseRequest : IRequest<RequestWithHandlerWithResponseResponse>
    {
        public string Data { get; set; } = null!;
    }

    public class RequestWithHandlerWithResponseResponse
    {
        public string Data { get; set; } = null!;
    }

    public class RequestWithHanlderWithoutResponseHandler : IRequestHandler<RequestWithHanlderWithoutResponseRequest>
    {
        public static int InvocationCount { get; private set; }

        public Task Handle(RequestWithHanlderWithoutResponseRequest request, CancellationToken cancellationToken)
        {
            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(RequestWithHanlderWithoutResponseHandler));

            return Task.CompletedTask;
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    public class RequestWithHandlerWithResponseHandler : IRequestHandler<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>
    {
        public static int InvocationCount { get; private set; }

        public Task<RequestWithHandlerWithResponseResponse> Handle(RequestWithHandlerWithResponseRequest request, CancellationToken cancellationToken)
        {
            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(RequestWithHandlerWithResponseHandler));

            return Task.FromResult(new RequestWithHandlerWithResponseResponse
            {
                Data = request.Data
            });
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    public class RequestFirstPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        public static int InvocationCount { get; private set; }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(RequestFirstPipelineBehavior<TRequest, TResponse>));

            return next();
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    public class RequestSecondPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    {
        public static int InvocationCount { get; private set; }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            InvocationCount++;
            PipelineExecutionOrder.Add(nameof(RequestSecondPipelineBehavior<TRequest, TResponse>));

            return next();
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    private readonly IRequestDispatcher _requestDispatcher;

    public RequestWithMultiplePipelinesTests()
    {
        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RequestWithHanlderWithoutResponseRequest>();

            config.AddRequestBehavior(typeof(RequestFirstPipelineBehavior<,>));
            config.AddRequestBehavior(typeof(RequestSecondPipelineBehavior<,>));
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _requestDispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();
    }

    // Tests for RequestWithoutResponse

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldInvokeHandler()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHanlderWithoutResponseHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldInvokeFirstPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldInvokeSecondPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestSecondPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldExecutePipelinesInCorrectOrder()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(3);
        PipelineExecutionOrder[0].ShouldBe(nameof(RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>));
        PipelineExecutionOrder[1].ShouldBe(nameof(RequestSecondPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>));
        PipelineExecutionOrder[2].ShouldBe(nameof(RequestWithHanlderWithoutResponseHandler));
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponseCalledMultipleTimes_ShouldIncrementAllInvocationCounts()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHanlderWithoutResponseHandler.InvocationCount.ShouldBe(2);
        RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.InvocationCount.ShouldBe(2);
        RequestSecondPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.InvocationCount.ShouldBe(2);
    }

    // Tests for RequestWithResponse

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldInvokeHandler()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHandlerWithResponseHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldInvokeFirstPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldInvokeSecondPipelineBehavior()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestSecondPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldExecutePipelinesInCorrectOrder()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(3);
        PipelineExecutionOrder[0].ShouldBe(nameof(RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>));
        PipelineExecutionOrder[1].ShouldBe(nameof(RequestSecondPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>));
        PipelineExecutionOrder[2].ShouldBe(nameof(RequestWithHandlerWithResponseHandler));
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldReturnCorrectData()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.Data.ShouldBe("TestData");
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseWithDifferentData_ShouldReturnMatchingData()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "DifferentData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.Data.ShouldBe("DifferentData");
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseCalledMultipleTimes_ShouldIncrementAllInvocationCounts()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHandlerWithResponseHandler.InvocationCount.ShouldBe(2);
        RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.InvocationCount.ShouldBe(2);
        RequestSecondPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.InvocationCount.ShouldBe(2);
    }

    // Helper method to reset all counters in one place
    private static void ResetAllCounters()
    {
        // Reset handler invocation counts
        RequestWithHanlderWithoutResponseHandler.ResetInvocationCount();
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();

        // Reset pipeline behavior invocation counts for RequestWithoutResponse
        RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.ResetInvocationCount();
        RequestSecondPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.ResetInvocationCount();

        // Reset pipeline behavior invocation counts for RequestWithResponse
        RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.ResetInvocationCount();
        RequestSecondPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.ResetInvocationCount();

        // Clear execution order tracking
        PipelineExecutionOrder.Clear();
    }
}
