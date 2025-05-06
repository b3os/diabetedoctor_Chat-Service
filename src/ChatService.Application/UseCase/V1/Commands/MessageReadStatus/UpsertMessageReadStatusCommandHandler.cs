using ChatService.Contract.Services.MessageReadStatus.Commands;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;

namespace ChatService.Application.UseCase.V1.Commands.MessageReadStatus;

public class UpsertMessageReadStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IMessageReadStatusRepository messageReadStatusRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository)
    : ICommandHandler<UpsertMessageReadStatusCommand>
{
    public async Task<Result> Handle(UpsertMessageReadStatusCommand request, CancellationToken cancellationToken)
    {
        var userExist = await userRepository.ExistsAsync(user => user.UserId.Id.Equals(request.UserId), cancellationToken);

        if (!userExist)
        {
            throw new UserExceptions.UserNotFoundException();
        }
        
        var groupExist = await groupRepository.ExistsAsync(group => group.Id == request.GroupId, cancellationToken);

        if (!groupExist)
        {
            throw new GroupExceptions.GroupNotFoundException();
        }
        
        var messageStatus = await messageReadStatusRepository.FindSingleAsync(
            status => status.UserId.Id == request.UserId 
                      && status.GroupId == request.GroupId, cancellationToken: cancellationToken);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            if (messageStatus is null)
            {
                var newMessageStatus = MapToDomain(request);
                await messageReadStatusRepository.CreateAsync(unitOfWork.ClientSession, newMessageStatus, cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            else
            {
                messageStatus.Update(request.MessageId);
                await messageReadStatusRepository.UpdateOneAsync(unitOfWork.ClientSession, messageStatus, cancellationToken);
            }
        }
        catch (Exception)
        {
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success();
    }

    private Domain.Models.MessageReadStatus MapToDomain(UpsertMessageReadStatusCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var userId = UserId.Of(command.UserId);
        return Domain.Models.MessageReadStatus.Create(id, userId, command.GroupId, command.MessageId);
    }
}