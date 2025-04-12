using Conduit.Constants;
using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Handlers;
using Conduit.Contract.Models;
using Conduit.Contract.Requests;
using Conduit.Dispatchers.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Dispatchers;

/// <summary>
/// Dispatches requests to their respective handlers and applies request pipeline behaviors.
/// </summary>
public class RequestDispatcher(IServiceProvider _serviceProvider) : IRequestDispatcher
{
    /// <summary>
    /// Sends a request without expecting a response, applying any configured pipeline behaviors.
    /// </summary>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handler or pipeline behavior is found, or if the handler does not return the expected Task.
    /// </exception>
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

            var handleMethod = currentBehavior?.GetType().GetMethod(ConduitConstants.Handle)
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

    /// <summary>
    /// Sends a request and expects a response, applying any configured pipeline behaviors.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected from the request.</typeparam>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response from the request handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handler or pipeline behavior is found, or if the handler does not return the expected Task.
    /// </exception>
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

            var handleMethod = currentBehavior?.GetType().GetMethod(ConduitConstants.Handle)
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

    /// <summary>
    /// Invokes the handler for a request without expecting a response.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handler is found or if the handler does not return the expected Task.
    /// </exception>
    private async Task<Unit> InvokeHandler(IRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var handler = _serviceProvider.GetRequiredService(handlerType)
            ?? throw new InvalidOperationException($"Handler for {requestType.Name} was not found.");

        var method = handlerType.GetMethod(ConduitConstants.Handle)
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task taskResult)
            throw new InvalidOperationException($"Handler for {requestType.Name} did not return the expected Task.");

        await taskResult;

        return Unit.Value;
    }

    /// <summary>
    /// Invokes the handler for a request and expects a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected from the request.</typeparam>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response from the request handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handler is found or if the handler does not return the expected Task.
    /// </exception>
    private async Task<TResponse> InvokeHandler<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType)
            ?? throw new InvalidOperationException($"Handler for {requestType.Name} was not found.");

        var method = handlerType.GetMethod(ConduitConstants.Handle)
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task<TResponse> taskResult)
            throw new InvalidOperationException($"Handler for {requestType.Name} did not return the expected Task<{typeof(TResponse).Name}>.");

        return await taskResult;
    }
}
