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
        var groupId = ObjectId.Parse(request.GroupId);
        var groupExist = await groupRepository.ExistsAsync(
            group => group.Id == groupId
                     && group.Admins.Any(userId => userId.Id.Equals(request.AdminId)),
            cancellationToken);
        
        if (!groupExist)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }
        
        var members = (await userRepository.FindListAsync(user => request.UserIds.Contains(user.UserId.Id), cancellationToken)).ToList();

        if (members.Count != request.UserIds.Count())
        {
            throw new UserExceptions.UserNotFoundException();
        }
        
        var memberUserIds = UserId.All(request.UserIds);
        var update = Builders<Domain.Models.Group>.Update.PullFilter(group => group.Members, member => memberUserIds.Contains(member));

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, groupId, update, cancellationToken);
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
}