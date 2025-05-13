using ChatService.Contract.DTOs.UserDTOs;
using ChatService.Contract.Services.Group.DomainEvents;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Models;
using ChatService.Domain.ValueObjects;
using ChatService.Persistence;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class AddMemberToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IPublisher publisher)
    : ICommandHandler<AddMemberToGroupCommand, Response<DuplicatedUserDto>>
{
    public async Task<Result<Response<DuplicatedUserDto>>> Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
    {
        var groupExist = await groupRepository.ExistsAsync(
            group => group.Id == request.GroupId
                     && group.Members.Any(member => member.UserId.Id == request.AdminId
                                                    && (member.Role == GroupRoleEnum.Admin ||
                                                        member.Role == GroupRoleEnum.Owner)),
            cancellationToken);

        if (!groupExist)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }

        var member = await userRepository.CountAsync(user => request.UserIds.Contains(user.UserId.Id), cancellationToken);

        if (member != request.UserIds.Count)
        {
            throw new UserExceptions.UserNotFoundException();
        }
        
        var duplicationResult = await groupRepository.CheckDuplicatedUsersAsync(request.GroupId, request.UserIds, cancellationToken);

        if (duplicationResult is not null && duplicationResult["matchCount"].AsInt32 > 0)
        {
            var userDuplicationResult = BsonSerializer.Deserialize<DuplicatedUserDto>(duplicationResult);
            return Result.Success(new Response<DuplicatedUserDto>(GroupMessage.GroupMemberAlreadyExists.GetMessage().Code,
                GroupMessage.GroupMemberAlreadyExists.GetMessage().Message, userDuplicationResult));
        }
        
        var memberUserIds = UserId.All(request.UserIds);
        var membersToAdd = Member.CreateMany(memberUserIds);
        var update = Builders<Domain.Models.Group>.Update.AddToSetEach(group => group.Members, membersToAdd);
        var options = new UpdateOptions<Domain.Models.Group> { IsUpsert = false };
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, request.GroupId, update, options, cancellationToken);
            await publisher.Publish(MapToEvent(request), cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response<DuplicatedUserDto>(GroupMessage.AddMemberToGroupSuccessfully.GetMessage().Code,
            GroupMessage.AddMemberToGroupSuccessfully.GetMessage().Message, new DuplicatedUserDto()));
    }

    private GroupMembersAddedEvent MapToEvent(AddMemberToGroupCommand command)
    {
        return new GroupMembersAddedEvent()
        {
            GroupId = command.GroupId.ToString(),
            Members = command.UserIds
        };
    }
}