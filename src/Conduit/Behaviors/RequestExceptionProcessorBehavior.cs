using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Conduit.Behaviors;

public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap reflection exceptions
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            // Unwrap single inner exception from AggregateException
            ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
            throw;
        }
    }
}
