using Conduit.Contract.Requests;

namespace Conduit.Dispatchers.Contracts;

public interface IRequestDispatcher
{
    Task Send(IRequest request, CancellationToken cancellationToken = default);

    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
