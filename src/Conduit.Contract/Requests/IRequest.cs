namespace Conduit.Contract.Requests;

/// <summary>
/// Represents a request that does not return a response.
/// </summary>
public interface IRequest : IBaseRequest;

/// <summary>
/// Represents a request that returns a response of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public interface IRequest<out TResponse> : IBaseRequest;
