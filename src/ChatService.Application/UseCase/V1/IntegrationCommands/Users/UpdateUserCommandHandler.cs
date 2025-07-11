using ChatService.Application.Mapping;
using ChatService.Contract.Services.User.Commands;

namespace ChatService.Application.UseCase.V1.IntegrationCommands.Users;

public class UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.UserId.Id.Equals(request.Id), cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new UserExceptions.UserNotFoundException();
        }

        var avatar = !string.IsNullOrWhiteSpace(request.Avatar) ? Image.Of("avatar",request.Avatar) : null;
        var fullname = request.FullName is not null ? Mapper.MapFullName(request.FullName) : null;
        user.Modify(fullname, avatar, request.PhoneNumber);
        
        await userRepository.ReplaceOneAsync(unitOfWork.ClientSession, user, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
            
        return Result.Success();
    }
}