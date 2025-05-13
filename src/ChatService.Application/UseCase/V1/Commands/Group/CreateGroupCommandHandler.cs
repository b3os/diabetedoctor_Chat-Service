using ChatService.Contract.Exceptions;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Group;
using ChatService.Contract.Services.Group.DomainEvents;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public sealed class CreateGroupCommandHandler(
    IGroupRepository groupRepository, 
    IUserRepository userRepository,
    IUnitOfWork unitOfWork, 
    IPublisher publisher)
    : ICommandHandler<CreateGroupCommand>
{
    public async Task<Result> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        await EnsureUserExistsAsync(request.OwnerId!, cancellationToken);
        
        var group = MapToGroup(request.OwnerId!, request);
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.CreateAsync(unitOfWork.ClientSession, group, cancellationToken);
            await publisher.Publish(MapToEvent(group), cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success(new Response(GroupMessage.CreatedGroupSuccessfully.GetMessage().Code,
            GroupMessage.CreatedGroupSuccessfully.GetMessage().Message));
    }

    private async Task EnsureUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsAsync(user => user.UserId.Id == userId, cancellationToken);
        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }

    private Domain.Models.Group MapToGroup(string ownerId, CreateGroupCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var avatar = Image.Of(command.Avatar);
        var ownerUserId = UserId.Of(ownerId);
        var memberIds = UserId.All(command.Members!.Where(userId => userId != ownerId));
        return Domain.Models.Group.Create(id, command.Name!, avatar, ownerUserId, memberIds);
    }

    private GroupCreatedEvent MapToEvent(Domain.Models.Group group)
    {
        return new GroupCreatedEvent(){GroupId = group.Id.ToString(), Name = group.Name, Members = group.Members.Select(member => member.UserId.Id)};
    }
}