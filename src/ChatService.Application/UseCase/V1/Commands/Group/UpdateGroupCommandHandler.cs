using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObject;
using MongoDB.Bson;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class UpdateGroupCommandHandler (IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateGroupCommand>
{
    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.FindByIdAsync(ObjectId.Parse(request.GroupId), cancellationToken);
        if (group is null)
        {
            throw new GroupExceptions.GroupNotFoundException();
        }

        group.Modify(request.GroupUpdateDto.Name,
            string.IsNullOrWhiteSpace(request.GroupUpdateDto.Avatar) ? null : Image.Of(request.GroupUpdateDto.Avatar));
        
        await unitOfWork.StartTransactionAsync();
        try
        {
            await groupRepository.ReplaceOneAsync(group.Id, group, cancellationToken);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }

        return Result.Success(new Response(GroupMessage.UpdatedGroupSuccessfully.GetMessage().Code,
            GroupMessage.UpdatedGroupSuccessfully.GetMessage().Message));
    }
}