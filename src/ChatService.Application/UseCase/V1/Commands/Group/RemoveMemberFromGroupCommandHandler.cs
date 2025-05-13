using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;
    
namespace ChatService.Application.UseCase.V1.Commands.Group;

public class RemoveMemberFromGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IGroupRepository groupRepository,
    IUserRepository userRepository) : ICommandHandler<RemoveMemberFromGroupCommand>
{
    public async Task<Result> Handle(RemoveMemberFromGroupCommand request, CancellationToken cancellationToken)
    {
        await EnsureUserExistsAsync(request.MemberId, cancellationToken);
        
        await GetGroupWithPermissionAsync(request.GroupId, request.AdminId!, cancellationToken);
        
        
        
        var update = await BuildRemoveMemberUpdate(request.GroupId, request.AdminId!, request.MemberId);
        var updateOptions = new UpdateOptions<Domain.Models.Group>() { IsUpsert = false };
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, request.GroupId, update, updateOptions, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(GroupMessage.RemoveMemberFromGroupSuccessfully.GetMessage().Code,
            GroupMessage.RemoveMemberFromGroupSuccessfully.GetMessage().Message));
    }
    
    private async Task EnsureUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsAsync(u => u.UserId.Id == userId, cancellationToken);
        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }
    
    private async Task GetGroupWithPermissionAsync(ObjectId groupId, string executorId, CancellationToken cancellationToken)
    {
        var exists= await groupRepository.ExistsAsync(
            group => group.Id == groupId
                     && group.Members.Any(member => member.UserId.Id == executorId
                                                    && (member.Role == GroupRoleEnum.Admin ||
                                                        member.Role == GroupRoleEnum.Owner)),
            cancellationToken);

        if (!exists)
            throw new GroupExceptions.GroupAccessDeniedException();
    }
    
    private async Task<UpdateDefinition<Domain.Models.Group>> BuildRemoveMemberUpdate(ObjectId groupId, string executorId, string memberId)
    {
        var members = await groupRepository.GetExecutorAndTarget(groupId, executorId, memberId);

        if (!members.TryGetValue(memberId, out var targetRole))
            throw new GroupExceptions.GroupMemberNotExistsException();
        
        if(members[executorId] >= targetRole)
            throw new GroupExceptions.CannotRemoveMemberException();

        var builder = Builders<Domain.Models.Group>.Update;
        // var updates = builder.Pull(group => group.Members.Where(member => member.UserId == memberUserId));
        var updates = builder.PullFilter(group => group.Members, member => member.UserId.Id == memberId);
        return builder.Combine(updates, builder.Set(group => group.ModifiedDate, DateTime.UtcNow));
    }
}