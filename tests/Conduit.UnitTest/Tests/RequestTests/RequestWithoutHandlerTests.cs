using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Conduit.UnitTest.Tests.RequestTests;

public class RequestWithoutHandlerTests
{
    public class RequestWithoutHanlderWithoutResponseRequest : IRequest
    {
        public string Data { get; set; } = null!;
    }

    public class RequestWithoutHandlerWithResponseRequest : IRequest<RequestWithoutHandlerWithResponseResponse>
    {
        public string Data { get; set; } = null!;
    }

    public class RequestWithoutHandlerWithResponseResponse
    {
        public string Data { get; set; } = null!;
    }

    private readonly IRequestDispatcher _requestDispatcher;

    public RequestWithoutHandlerTests()
    {

        var services = new ServiceCollection();

        services.AddConduit(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RequestWithoutHanlderWithoutResponseRequest>();
        });

        ServiceProvider serviceProvider;
        serviceProvider = services.BuildServiceProvider();
        _requestDispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();
    }

    // Tests for RequestWithoutResponse

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithoutResponse_ShouldThrowException()
    {
        // Arrange
        var request = new RequestWithoutHanlderWithoutResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithoutResponse_ShouldThrowInvalidOperationExceptionType()
    {
        // Arrange
        var request = new RequestWithoutHanlderWithoutResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));

        exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithoutResponse_ShouldHaveDescriptiveExceptionMessage()
    {
        // Arrange
        var request = new RequestWithoutHanlderWithoutResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));

        exception.Message.ShouldContain("handler");
        exception.Message.ShouldContain(typeof(RequestWithoutHanlderWithoutResponseRequest).FullName!);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithoutResponseWithNullData_ShouldStillThrowException()
    {
        // Arrange
        var request = new RequestWithoutHanlderWithoutResponseRequest
        {
            Data = null!
        };

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));
    }

    // Tests for RequestWithResponse

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithResponse_ShouldThrowException()
    {
        // Arrange
        var request = new RequestWithoutHandlerWithResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithResponse_ShouldThrowInvalidOperationExceptionType()
    {
        // Arrange
        var request = new RequestWithoutHandlerWithResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));

        exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithResponse_ShouldHaveDescriptiveExceptionMessage()
    {
        // Arrange
        var request = new RequestWithoutHandlerWithResponseRequest
        {
            Data = "Test"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));

        exception.Message.ShouldContain("handler");
        exception.Message.ShouldContain(typeof(RequestWithoutHandlerWithResponseRequest).FullName!);
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithResponseWithNullData_ShouldStillThrowException()
    {
        // Arrange
        var request = new RequestWithoutHandlerWithResponseRequest
        {
            Data = null!
        };

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request));
    }

    // Test cancellation token behavior

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithoutResponseAndCancellationToken_ShouldThrowException()
    {
        // Arrange
        var request = new RequestWithoutHanlderWithoutResponseRequest { Data = "Test" };
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request, cancellationToken));
    }

    [Fact]
    public async Task Send_WhenRequestWithoutHandlerWithResponseAndCancellationToken_ShouldThrowException()
    {
        // Arrange
        var request = new RequestWithoutHandlerWithResponseRequest { Data = "Test" };
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await _requestDispatcher.Send(request, cancellationToken));
    }
}
