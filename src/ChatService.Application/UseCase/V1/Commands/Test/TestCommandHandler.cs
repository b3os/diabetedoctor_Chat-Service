using System.Threading;
using System.Threading.Tasks;
using ChatService.Contract.Services.User;

namespace ChatService.Application.UseCase.V1.Commands.Test;

public sealed class TestCommandHandler
    : ICommandHandler<TestCommand>
{
    public async Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Result.Success(new Success("123", "Success"));
    }
}