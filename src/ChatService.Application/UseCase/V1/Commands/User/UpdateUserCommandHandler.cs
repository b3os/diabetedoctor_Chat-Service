using ChatService.Contract.Services.User.Commands;

namespace ChatService.Application.UseCase.V1.Commands.User;

public class UpdateUserCommandHandler (IUserRepository userRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.UserId.Id.Equals(request.Id), cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new UserExceptions.UserNotFoundException();
        }

        user.Modify(request.FullName, string.IsNullOrWhiteSpace(request.Avatar) ? null : Image.Of(request.Avatar));
        
        await userRepository.ReplaceOneAsync(unitOfWork.ClientSession, user, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
            
        return Result.Success();
    }
}