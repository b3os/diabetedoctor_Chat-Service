using ChatService.Contract.Services.User;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObject;
using MongoDB.Bson;

namespace ChatService.Application.UseCase.V1.Commands.User;

public class CreateUserCommandHandler (IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = MapToUser(request);
        
        await unitOfWork.StartTransactionAsync();
        try
        {
            await userRepository.CreateAsync(unitOfWork.ClientSession, user, cancellationToken);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }
        
        return Result.Success();
    }

    private Domain.Models.User MapToUser(CreateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var id = ObjectId.GenerateNewId();
        return Domain.Models.User.Create(id: id, command.UserId.ToString(), command.FullName, Image.Of(command.Avatar));
    }
}