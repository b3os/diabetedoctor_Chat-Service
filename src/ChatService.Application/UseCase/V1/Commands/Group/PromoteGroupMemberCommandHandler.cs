using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class PromoteGroupMemberCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<PromoteGroupMemberCommand>
{
    public async Task<Result> Handle(PromoteGroupMemberCommand request, CancellationToken cancellationToken)
    {
        // var group = await groupRepository
        //     .FindSingleAsync(
        //         group => group.Id == ObjectId.Parse(request.GroupId) &
        //                  group.Members.Exists(member => member.Equals(request.MemberId)), cancellationToken);
        // if (group is null)
        // {
        //     throw new GroupExceptions.GroupNotFoundException();
        // }
        return Result.Success();
    }
}