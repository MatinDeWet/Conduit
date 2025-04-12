namespace Conduit.Contract.Delegates;

/// <summary>
/// Represents a delegate that defines the next action in a notification pipeline.
/// </summary>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task NotificationHandlerDelegate();
