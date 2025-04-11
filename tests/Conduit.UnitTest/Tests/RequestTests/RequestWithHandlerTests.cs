using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.RequestTests;

public class RequestWithHandlerTests
{
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

            return Task.FromResult(new RequestWithHandlerWithResponseResponse
            {
                Data = request.Data
            });
        }

        public static void ResetInvocationCount() => InvocationCount = 0;
    }

    private readonly IRequestDispatcher _requestDispatcher;

    public RequestWithHandlerTests()
    {

        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RequestWithHanlderWithoutResponseRequest>();
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _requestDispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();
    }

    // Tests for RequestWithoutResponse

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldCallDispatcher()
    {
        // Arrange
        RequestWithHanlderWithoutResponseHandler.ResetInvocationCount();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert - This test just confirms the call completes without errors
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponse_ShouldInvokeHandlerExactlyOnce()
    {
        // Arrange
        RequestWithHanlderWithoutResponseHandler.ResetInvocationCount();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHanlderWithoutResponseHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutResponseCalledMultipleTimes_ShouldInvokeHandlerMultipleTimes()
    {
        // Arrange
        RequestWithHanlderWithoutResponseHandler.ResetInvocationCount();
        var request = new RequestWithHanlderWithoutResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHanlderWithoutResponseHandler.InvocationCount.ShouldBe(3);
    }

    // Tests for RequestWithResponse

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldCallDispatcher()
    {
        // Arrange
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldInvokeHandlerExactlyOnce()
    {
        // Arrange
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHandlerWithResponseHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task Send_WhenRequestWithResponse_ShouldReturnCorrectResponseData()
    {
        // Arrange
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.Data.ShouldBe("TestData");
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseWithDifferentData_ShouldReturnMatchingResponseData()
    {
        // Arrange
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        var request = new RequestWithHandlerWithResponseRequest { Data = "DifferentData" };

        // Act
        var response = await _requestDispatcher.Send(request);

        // Assert
        response.Data.ShouldBe("DifferentData");
    }

    [Fact]
    public async Task Send_WhenRequestWithResponseCalledMultipleTimes_ShouldInvokeHandlerMultipleTimes()
    {
        // Arrange
        RequestWithHandlerWithResponseHandler.ResetInvocationCount();
        var request = new RequestWithHandlerWithResponseRequest { Data = "TestData" };

        // Act
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);
        await _requestDispatcher.Send(request);

        // Assert
        RequestWithHandlerWithResponseHandler.InvocationCount.ShouldBe(3);
    }
}
