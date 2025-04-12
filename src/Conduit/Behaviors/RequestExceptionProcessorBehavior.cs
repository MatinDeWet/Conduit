using Conduit.Contract.Behaviors;
using Conduit.Contract.Delegates;
using Conduit.Contract.Requests;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Conduit.Behaviors;

/// <summary>
/// A pipeline behavior that processes exceptions thrown during request handling.
/// Unwraps common exception types to expose the original exception details.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request handler</typeparam>
public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{

    /// <summary>
    /// Executes the next delegate in the request pipeline and handles specific exception cases.
    /// Unwraps TargetInvocationException and AggregateException with single inner exceptions
    /// to simplify debugging and error handling.
    /// </summary>
    /// <param name="request">The request being processed</param>
    /// <param name="next">The next handler delegate in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation</param>
    /// <returns>The response from the request handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
            throw;
        }
    }
}
