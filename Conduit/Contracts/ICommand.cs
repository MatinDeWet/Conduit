namespace Conduit.Contracts;

/// <summary>
/// Marker interface for a command that does not return a response payload.
/// </summary>
public interface ICommand;

/// <summary>
/// Marker interface for a command that returns a response payload.
/// </summary>
/// <typeparam name="TResponse">The expected response type.</typeparam>
public interface ICommand<TResponse>;
