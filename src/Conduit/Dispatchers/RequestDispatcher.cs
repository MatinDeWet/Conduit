using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Models;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Dispatchers;

public class RequestDispatcher(IServiceProvider _serviceProvider) : IRequestDispatcher
{
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var behaviors = _serviceProvider.GetServices(typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, typeof(Unit)))
            .ToList();

        RequestHandlerDelegate<Unit> pipeline = () => InvokeHandler(request, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var nextDelegate = pipeline;

            var handleMethod = currentBehavior?.GetType().GetMethod("Handle")
                ?? throw new InvalidOperationException($"Handle method not found on behavior of type {currentBehavior?.GetType().Name}.");

            pipeline = () =>
            {
                var result = handleMethod.Invoke(currentBehavior, [request, nextDelegate, cancellationToken]);
                if (result is not Task<Unit> taskResult)
                {
                    throw new InvalidOperationException($"Handle method did not return the expected Task<{typeof(Unit).Name}>.");
                }
                return taskResult;
            };
        }

        await pipeline();
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var behaviors = _serviceProvider.GetServices(typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, typeof(TResponse)))
            .ToList();

        RequestHandlerDelegate<TResponse> pipeline = () => InvokeHandler<TResponse>(request, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var nextDelegate = pipeline;

            var handleMethod = currentBehavior?.GetType().GetMethod("Handle")
                ?? throw new InvalidOperationException($"Handle method not found on behavior of type {currentBehavior?.GetType().Name}.");

            pipeline = () =>
            {
                var result = handleMethod.Invoke(currentBehavior, [request, nextDelegate, cancellationToken]);
                if (result is not Task<TResponse> taskResult)
                {
                    throw new InvalidOperationException($"Handle method did not return the expected Task<{typeof(TResponse).Name}>.");
                }
                return taskResult;
            };
        }

        return await pipeline();
    }

    private async Task<Unit> InvokeHandler(IRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var handler = _serviceProvider.GetRequiredService(handlerType)
            ?? throw new InvalidOperationException($"Handler for {requestType.Name} was not found.");

        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task taskResult)
            throw new InvalidOperationException($"Handler for {requestType.Name} did not return the expected Task.");

        await taskResult;

        return Unit.Value;
    }

    private async Task<TResponse> InvokeHandler<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType)
            ?? throw new InvalidOperationException($"Handler for {requestType.Name} was not found.");

        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task<TResponse> taskResult)
            throw new InvalidOperationException($"Handler for {requestType.Name} did not return the expected Task<{typeof(TResponse).Name}>.");

        return await taskResult;
    }
}
