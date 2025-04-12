using Conduit.Contract.Requests;

namespace Conduit.Dispatchers.Contracts;

/// <summary>
/// Defines a dispatcher for sending requests to their respective handlers.
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>
    /// Sends a request that does not return a response to its handler.
    /// </summary>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no handler is found for the request.</exception>
    Task Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request that returns a response to its handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the request.</typeparam>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the response from the handler.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no handler is found for the request.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
