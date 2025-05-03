using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class PromoteGroupMemberCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<PromoteGroupMemberCommand>
{
    public async Task<Result> Handle(PromoteGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.FindSingleAsync(
                group => group.Id == ObjectId.Parse(request.GroupId) 
                         && group.Owner.Id == request.OwnerId
                         && !group.Admins.Any(admin => admin.Id.Equals(request.MemberId))
                         && group.Members.Any(member => member.Id.Equals(request.MemberId)), 
                cancellationToken: cancellationToken);
        
        if (group is null)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }
        
        var memberUserId = UserId.Of(request.MemberId);
        group.AddAdmin(memberUserId);
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.AddToSetEach<UserId>(unitOfWork.ClientSession, group, cancellationToken);
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