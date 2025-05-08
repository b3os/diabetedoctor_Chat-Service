using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class UpdateGroupCommandHandler (IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateGroupCommand>
{
    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var groupId = ObjectId.Parse(request.GroupId);
        var projection = Builders<Domain.Models.Group>.Projection.Include(group => group.Name).Include(group => group.Avatar);
        var group = await groupRepository.FindSingleAsync(
            group => group.Id == groupId 
                 && group.Admins.Any(userId => userId.Id.Equals(request.AdminId)),
            projection,
            cancellationToken);
        
        if (group is null)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }

        var updates = new List<UpdateDefinition<Domain.Models.Group>>();

        if (!string.IsNullOrWhiteSpace(request.Name) &&
            !request.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase))
        {
            updates.Add(Builders<Domain.Models.Group>.Update.Set(x => x.Name, request.Name));
        }

        if (!string.IsNullOrWhiteSpace(request.Avatar))
        {
            updates.Add(Builders<Domain.Models.Group>.Update.Set(x => x.Avatar, Image.Of(request.Avatar)));
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, groupId, Builders<Domain.Models.Group>.Update.Combine(updates), cancellationToken);
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