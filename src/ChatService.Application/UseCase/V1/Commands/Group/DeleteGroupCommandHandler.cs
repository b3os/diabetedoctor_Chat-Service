using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class DeleteGroupCommandHandler(
    IUnitOfWork unitOfWork, 
    IGroupRepository groupRepository, 
    IMessageRepository messageRepository, 
    IMessageReadStatusRepository messageReadStatusRepository,
    IPublisher publisher) 
    : ICommandHandler<DeleteGroupCommand>
{
    public async Task<Result> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        await EnsureGroupOwnerAccessAsync(request.GroupId, request.OwnerId!, cancellationToken);
        
        var messageFilter = Builders<Domain.Models.Message>.Filter.Eq(g => g.GroupId, request.GroupId);
        var messageStatusFilter = Builders<Domain.Models.MessageReadStatus>.Filter.Eq(g => g.GroupId, request.GroupId);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.DeleteOneAsync(unitOfWork.ClientSession, request.GroupId, cancellationToken);
            await messageRepository.DeleteManyAsync(unitOfWork.ClientSession, messageFilter, cancellationToken);
            await messageReadStatusRepository.DeleteManyAsync(unitOfWork.ClientSession, messageStatusFilter, cancellationToken);
            await publisher.Publish(MapToEvent(request.GroupId), cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(GroupMessage.DeleteGroupSuccessfully.GetMessage().Code,
            GroupMessage.DeleteGroupSuccessfully.GetMessage().Message));
    }

    private async Task EnsureGroupOwnerAccessAsync(ObjectId groupId, string ownerId, CancellationToken cancellationToken)
    {
        var groupExist = await groupRepository.ExistsAsync(
            group => group.Id == groupId && group.Members.Any(m => m.UserId.Id == ownerId && m.Role == GroupRoleEnum.Owner),
            cancellationToken);

        if (!groupExist)
            throw new GroupExceptions.GroupAccessDeniedException();
    }

    private GroupDeletedEvent MapToEvent(ObjectId groupId)
    {
        return new GroupDeletedEvent()
        {
            GroupId = groupId.ToString(),
        };
    }
}