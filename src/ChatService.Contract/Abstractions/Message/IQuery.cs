using ChatService.Contract.Abstractions.Shared;

namespace ChatService.Contract.Abstractions.Message;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}