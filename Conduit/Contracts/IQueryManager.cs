using Ardalis.Result;

namespace Conduit.Contracts;

/// <summary>
/// Handles query requests and returns typed results.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryManager<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles a query instance.
    /// </summary>
    /// <param name="request">The query to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The operation result containing a response payload.</returns>
    Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken);
}
