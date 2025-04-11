using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;

namespace Conduit.Contract.Behaviors;

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IBaseRequest
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
