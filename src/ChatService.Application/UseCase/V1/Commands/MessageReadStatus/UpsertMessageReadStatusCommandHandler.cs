using ChatService.Contract.Services.MessageReadStatus.Commands;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.MessageReadStatus;

public class UpsertMessageReadStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IMessageReadStatusRepository messageReadStatusRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IMessageRepository messageRepository)
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

        var filter = Builders<Domain.Models.Message>.Filter.And(
            Builders<Domain.Models.Message>.Filter.Eq(message => message.GroupId, request.GroupId),
            Builders<Domain.Models.Message>.Filter.Gte(message => message.Id, request.MessageId));

        var userId = UserId.Of(request.UserId);
        var addToSet = Builders<Domain.Models.Message>.Update.AddToSet(message => message.ReadBy, userId);
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await messageRepository.UpdateManyAsync(unitOfWork.ClientSession, filter, addToSet, cancellationToken: cancellationToken);
            if (messageStatus is null)
            {
                var newMessageStatus = MapToDomain(request);
                await messageReadStatusRepository.CreateAsync(unitOfWork.ClientSession, newMessageStatus, cancellationToken);
            }
            else
            {
                var update = Builders<Domain.Models.MessageReadStatus>.Update.Set(messRead => messRead.LastReadMessageId, request.MessageId);
                await messageReadStatusRepository.UpdateOneAsync(unitOfWork.ClientSession, messageStatus.Id, update, cancellationToken);
            }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
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