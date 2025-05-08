using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using ChatService.Persistence;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class AddMemberToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IGroupRepository groupRepository,
    IUserRepository userRepository) : ICommandHandler<AddMemberToGroupCommand>
{
    public async Task<Result> Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
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

        var member = await userRepository.FindListAsync(user => request.UserIds.Contains(user.UserId.Id), cancellationToken);

        if (member.Count() != request.UserIds.Count())
        {
            throw new UserExceptions.UserNotFoundException();
        }

        var memberUserIds = UserId.All(request.UserIds);
        var update = Builders<Domain.Models.Group>.Update.Combine(
            Builders<Domain.Models.Group>.Update.AddToSetEach(group => group.Members, memberUserIds),
            Builders<Domain.Models.Group>.Update.Set(group => group.ModifiedDate, CurrentTimeService.GetCurrentTime()));
        
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

        return Result.Success();
    }
}