namespace Conduit.Contracts;

/// <summary>
/// Marker interface for a query that returns a response payload.
/// </summary>
/// <typeparam name="TResponse">The expected response type.</typeparam>
public interface IQuery<TResponse>;
