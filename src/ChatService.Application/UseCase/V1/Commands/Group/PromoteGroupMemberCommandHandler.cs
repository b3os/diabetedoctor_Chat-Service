using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class PromoteGroupMemberCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<PromoteGroupMemberCommand>
{
    public async Task<Result> Handle(PromoteGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var groupId = ObjectId.Parse(request.GroupId);
        var projection = Builders<Domain.Models.Group>.Projection.Include(group => group.Admins);
        var group = await groupRepository.FindSingleAsync(
                group => group.Id == groupId
                         && group.Owner.Id == request.OwnerId
                         && group.Members.Any(member => member.Id.Equals(request.MemberId)),
                projection,
                cancellationToken: cancellationToken);
        
        if (group is null)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }
        
        var isPromoted = !group.Admins.Any(admin => admin.Id.Equals(request.MemberId));
        var memberUserId = UserId.Of(request.MemberId);

        var builder = Builders<Domain.Models.Group>.Update;
        var update = isPromoted
            ? builder.AddToSet(x => x.Admins, memberUserId) 
            : builder.Pull(x => x.Admins, memberUserId);
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, groupId ,update, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success(new Response(GroupMessage.PromoteAdminGroupSuccessfully.GetMessage().Code,
            GroupMessage.PromoteAdminGroupSuccessfully.GetMessage().Message));
    }
}