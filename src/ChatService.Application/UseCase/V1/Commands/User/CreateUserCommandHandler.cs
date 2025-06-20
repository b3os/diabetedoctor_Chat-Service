using ChatService.Contract.Services.User.Commands;

namespace ChatService.Application.UseCase.V1.Commands.User;

public class CreateUserCommandHandler (IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = MapToUser(request);
        await userRepository.CreateAsync(unitOfWork.ClientSession, user, cancellationToken);
        return Result.Success();
    }

    private Domain.Models.User MapToUser(CreateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var id = ObjectId.GenerateNewId();
        var userId = UserId.Of(command.Id);
        var avatar = Image.Of(command.Avatar);
        return Domain.Models.User.Create(id: id, userId, command.FullName, avatar);
    }
}