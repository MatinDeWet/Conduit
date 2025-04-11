using BenchmarkDotNet.Attributes;
using Conduit.Behaviors;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Conduit.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class RequestBenchmarks
{
    private IRequestDispatcher _requestDispatcherWithoutPipelines = null!;
    private IRequestDispatcher _requestDispatcherWithOnePipeline = null!;
    private IRequestDispatcher _requestDispatcherWithTwoPipelines = null!;

    private SimpleRequest _smallRequest = null!;
    private SimpleRequest _mediumRequest = null!;
    private SimpleRequest _largeRequest = null!;

    private SimpleRequestWithResponse _smallRequestWithResponse = null!;
    private SimpleRequestWithResponse _mediumRequestWithResponse = null!;
    private SimpleRequestWithResponse _largeRequestWithResponse = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup dispatchers with different pipeline configurations
        _requestDispatcherWithoutPipelines = CreateDispatcher(0);
        _requestDispatcherWithOnePipeline = CreateDispatcher(1);
        _requestDispatcherWithTwoPipelines = CreateDispatcher(2);

        // Create requests with different payload sizes
        _smallRequest = new SimpleRequest { Data = CreateString(100) };
        _mediumRequest = new SimpleRequest { Data = CreateString(10_000) };
        _largeRequest = new SimpleRequest { Data = CreateString(1_000_000) };

        _smallRequestWithResponse = new SimpleRequestWithResponse { Data = CreateString(100) };
        _mediumRequestWithResponse = new SimpleRequestWithResponse { Data = CreateString(10_000) };
        _largeRequestWithResponse = new SimpleRequestWithResponse { Data = CreateString(1_000_000) };
    }

    private static string CreateString(int size)
    {
        var builder = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            builder.Append((char)('A' + (i % 26)));
        }
        return builder.ToString();
    }

    private static IRequestDispatcher CreateDispatcher(int pipelineCount)
    {
        var services = new ServiceCollection();

        // Add exception processor as the first pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>));

        services.AddConduit(config =>
        {
            // Register handlers
            config.RegisterServicesFromAssemblyContaining<SimpleRequestHandler>();

            // Add requested number of additional pipelines
            if (pipelineCount >= 1)
            {
                config.AddRequestBehavior(typeof(LoggingPipelineBehavior<,>));
            }

            if (pipelineCount >= 2)
            {
                config.AddRequestBehavior(typeof(ValidationPipelineBehavior<,>));
            }
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IRequestDispatcher>();
    }

    #region Request without Response

    [Benchmark]
    public async Task SimpleRequest_Small_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_smallRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Small_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_smallRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Small_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_smallRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Medium_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_mediumRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Medium_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_mediumRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Medium_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_mediumRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Large_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_largeRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Large_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_largeRequest);
    }

    [Benchmark]
    public async Task SimpleRequest_Large_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_largeRequest);
    }

    #endregion

    #region Request with Response

    [Benchmark]
    public async Task SimpleRequestWithResponse_Small_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_smallRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Small_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_smallRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Small_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_smallRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Medium_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_mediumRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Medium_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_mediumRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Medium_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_mediumRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Large_NoPipelines()
    {
        await _requestDispatcherWithoutPipelines.Send(_largeRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Large_OnePipeline()
    {
        await _requestDispatcherWithOnePipeline.Send(_largeRequestWithResponse);
    }

    [Benchmark]
    public async Task SimpleRequestWithResponse_Large_TwoPipelines()
    {
        await _requestDispatcherWithTwoPipelines.Send(_largeRequestWithResponse);
    }

    #endregion

    #region Support Classes

    public class SimpleRequest : IRequest
    {
        public string Data { get; set; } = null!;
    }

    public class SimpleRequestWithResponse : IRequest<SimpleResponse>
    {
        public string Data { get; set; } = null!;
    }

    public class SimpleResponse
    {
        public string ProcessedData { get; set; } = null!;
    }

    public class SimpleRequestHandler : IRequestHandler<SimpleRequest>
    {
        public Task Handle(SimpleRequest request, CancellationToken cancellationToken)
        {
            // Simulate minimal processing - we want to benchmark pipeline overhead
            _ = request.Data.Length;
            return Task.CompletedTask;
        }
    }

    public class SimpleRequestWithResponseHandler : IRequestHandler<SimpleRequestWithResponse, SimpleResponse>
    {
        public Task<SimpleResponse> Handle(SimpleRequestWithResponse request, CancellationToken cancellationToken)
        {
            // Simulate minimal processing - we want to benchmark pipeline overhead
            return Task.FromResult(new SimpleResponse
            {
                ProcessedData = request.Data[..Math.Min(10, request.Data.Length)]
            });
        }
    }

    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Simulate logging operation overhead
            var requestType = request.GetType().Name;
            _ = $"Processing request {requestType}";

            return next();
        }
    }

    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Simulate validation overhead
            if (request is SimpleRequest simpleRequest)
            {
                _ = !string.IsNullOrEmpty(simpleRequest.Data);
            }
            else if (request is SimpleRequestWithResponse simpleRequestWithResponse)
            {
                _ = !string.IsNullOrEmpty(simpleRequestWithResponse.Data);
            }

            return next();
        }
    }

    #endregion
}
