namespace Conduit.Contract.Requests;

public interface IRequest : IBaseRequest;

public interface IRequest<out TResponse> : IBaseRequest;
