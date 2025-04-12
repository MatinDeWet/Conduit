namespace Conduit.Contract.Delegates;

/// <summary>
/// Represents a delegate that defines the next action in a request pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request handler.</typeparam>
/// <returns>A task representing the asynchronous operation, with the response from the request handler.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
