using ChatService.Contract.Services.User.Commands;
using Mapper = ChatService.Application.Mapping.Mapper;

namespace ChatService.Application.UseCase.V1.IntegrationCommands.User;

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
        var hospitalId = !string.IsNullOrWhiteSpace(command.HospitalId) ? HospitalId.Of(command.HospitalId) : null;
        var fullName = Mapper.MapFullName(command.FullName);
        var avatar = Image.Of("default-avatar", command.Avatar);
        var role = Mapper.MapRoleFromInt(command.Role);
        return Domain.Models.User.Create(id, userId, hospitalId, fullName, avatar, command.PhoneNumber, role);
    }
}