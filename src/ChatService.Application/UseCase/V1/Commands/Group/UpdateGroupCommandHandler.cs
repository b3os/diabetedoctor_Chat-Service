using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class UpdateGroupCommandHandler (IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateGroupCommand>
{
    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.FindSingleAsync(x => x.Id == ObjectId.Parse(request.GroupId)
            && x.Admins.Any(userId => userId.Id.Equals(request.AdminId)),
            cancellationToken: cancellationToken);
        
        if (group is null)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }

        group.Modify(request.Name,
            string.IsNullOrWhiteSpace(request.Avatar) ? null : Image.Of(request.Avatar));
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, group, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(GroupMessage.UpdatedGroupSuccessfully.GetMessage().Code,
            GroupMessage.UpdatedGroupSuccessfully.GetMessage().Message));
    }
}