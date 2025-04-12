using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;

namespace Conduit.Contract.Behaviors;

/// <summary>
/// Defines a pipeline behavior for processing requests before they reach their handlers.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request handler.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IBaseRequest
{
    /// <summary>
    /// Handles the request by applying custom behavior and invoking the next delegate in the pipeline.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next delegate in the pipeline to invoke.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the response from the request handler.</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
