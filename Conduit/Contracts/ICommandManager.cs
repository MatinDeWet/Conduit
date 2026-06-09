using Ardalis.Result;

namespace Conduit.Contracts;

/// <summary>
/// Handles commands that do not produce a typed response.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandManager<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles a command instance.
    /// </summary>
    /// <param name="request">The command to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<Result> Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// Handles commands that produce a typed response.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommandManager<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles a command instance.
    /// </summary>
    /// <param name="request">The command to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The operation result with a response payload.</returns>
    Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken);
}
