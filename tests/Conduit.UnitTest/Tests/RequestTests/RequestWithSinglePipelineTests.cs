using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Models;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.RequestTests;

public class RequestWithSinglePipelineTests
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

    private class TestPipelineBehavior<TRequest, TResponse>(Func<TRequest, Task> _requestValidator) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await _requestValidator(request);
            return await next();
        }
    }

    private readonly IRequestDispatcher _requestDispatcher;

    public RequestWithSinglePipelineTests()
    {

        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RequestWithHanlderWithoutResponseRequest>();

            config.AddRequestBehavior(typeof(RequestFirstPipelineBehavior<,>));
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
    public async Task Send_WhenRequestWithoutResponse_ShouldInvokePipelineBehavior()
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
    public async Task Send_WhenRequestWithoutResponse_ShouldExecutePipelineBeforeHandler()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(2);
        PipelineExecutionOrder[0].ShouldBe(nameof(RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>));
        PipelineExecutionOrder[1].ShouldBe(nameof(RequestWithHanlderWithoutResponseHandler));
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponseCalledMultipleTimes_ShouldIncrementHandlerInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHanlderWithoutResponseHandler.InvocationCount.ShouldBe(2);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponseCalledMultipleTimes_ShouldIncrementPipelineInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.InvocationCount.ShouldBe(2);
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
    public async Task Send_WhenRequestWithResponse_ShouldInvokePipelineBehavior()
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
    public async Task Send_WhenRequestWithResponse_ShouldExecutePipelineBeforeHandler()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        PipelineExecutionOrder.Count.ShouldBe(2);
        PipelineExecutionOrder[0].ShouldBe(nameof(RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>));
        PipelineExecutionOrder[1].ShouldBe(nameof(RequestWithHandlerWithResponseHandler));
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldReturnResponseWithCorrectData()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldBe("TestData");
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseWithDifferentData_ShouldReturnMatchingResponseData()
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
    public async Task Send_WhenRequestWithResponseCalledMultipleTimes_ShouldIncrementHandlerInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHandlerWithResponseHandler.InvocationCount.ShouldBe(2);
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseCalledMultipleTimes_ShouldIncrementPipelineInvocationCount()
    {
        // Arrange
        ResetAllCounters();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.InvocationCount.ShouldBe(2);
    }

    // Additional tests for pipeline behavior specifics

    [Fact]
    public async Task Send_WhenRequestWithResponse_PipelineShouldHaveAccessToRequestData()
    {
        // Arrange
        ResetAllCounters();

        // Create a custom pipeline behavior that verifies request data
        bool pipelineReceivedCorrectData = false;

        var services = new ServiceCollection();
        services.AddTransient(typeof(IPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>),
            _ => new TestPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>(
                request =>
                {
                    pipelineReceivedCorrectData = request.Data == "SpecialData";
                    return Task.CompletedTask;
                }));

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RequestWithHanlderWithoutResponseRequest>();
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();

        var request = new RequestWithHandlerWithResponseRequest { Data = "SpecialData" };

        // Act
        await dispatcher.Send(request);

        // Assert
        pipelineReceivedCorrectData.ShouldBeTrue();
    }

    // Helper method to reset all counters in one place
    private static void ResetAllCounters()
    {
        RequestWithHanlderWithoutResponseHandler.ResetInvocationCount();
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        RequestFirstPipelineBehavior<RequestWithHanlderWithoutResponseRequest, Unit>.ResetInvocationCount();
        RequestFirstPipelineBehavior<RequestWithHandlerWithResponseRequest, RequestWithHandlerWithResponseResponse>.ResetInvocationCount();
        PipelineExecutionOrder.Clear();
    }
}
