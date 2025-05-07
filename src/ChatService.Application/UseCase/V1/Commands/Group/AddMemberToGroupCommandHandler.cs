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
        var projection = Builders<Domain.Models.Group>.Projection.Include(group => group.Id);
        var group = await groupRepository.FindSingleAsync(
            group => group.Id == ObjectId.Parse(request.GroupId)
                     && group.Admins.Any(userId => userId.Id.Equals(request.AdminId))
                     && !group.Members.Any(member => request.UserIds.Contains(member.Id)),
            projection,
            cancellationToken);

        if (group is null)
        {
            throw new GroupExceptions.GroupMemberAlreadyExistsException();
        }

        var member =
            await userRepository.FindListAsync(user => request.UserIds.Contains(user.UserId.Id), cancellationToken);

        if (member.Count() != request.UserIds.Count())
        {
            throw new UserExceptions.UserNotFoundException();
        }

        var memberUserIds = UserId.All(request.UserIds);
        group.AddMembers(memberUserIds);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync<UserId>(unitOfWork.ClientSession, group, cancellationToken);
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