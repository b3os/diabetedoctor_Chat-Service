using ChatService.Contract.Services.User;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObject;

namespace ChatService.Application.UseCase.V1.Commands.User;

public class UpdateUserCommandHandler (IUserRepository userRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.UserId.Equals(request.UserId), cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure(new Error("ABC",$"User with id {request.UserId} does not exist."));
        }

        user.Modify(request.FullName, string.IsNullOrWhiteSpace(request.Avatar) ? null : Image.Of(request.Avatar));

        await unitOfWork.StartTransactionAsync();
        try
        {
            await userRepository.ReplaceOneAsync(user.Id, user, cancellationToken);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }
        
        return Result.Success();
    }
}