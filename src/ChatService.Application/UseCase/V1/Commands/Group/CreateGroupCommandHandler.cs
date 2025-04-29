using ChatService.Application.Infrastructure.Abstractions;
using ChatService.Contract.Exceptions;
using ChatService.Contract.Services.Group;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObject;
using MongoDB.Bson;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public sealed class CreateGroupCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork, IClaimsService claimsService)
    : ICommandHandler<CreateGroupCommand>
{
    public async Task<Result> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var ownerId = claimsService.GetCurrentUserId;
        
        var group = MapToGroup(ownerId, request);
        
        await unitOfWork.StartTransactionAsync();
        try
        {
            await groupRepository.CreateAsync(unitOfWork.ClientSession, group, cancellationToken);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }
        
        return Result.Success(new Response(GroupMessage.CreatedGroupSuccessfully.GetMessage().Code,
            GroupMessage.CreatedGroupSuccessfully.GetMessage().Message));
    }

    private Domain.Models.Group MapToGroup(string ownerId, CreateGroupCommand command)
    {
        var id = ObjectId.GenerateNewId();
        return Domain.Models.Group.Create(id, command.Group.Name, Image.Of(command.Group.Avatar), ownerId,
            command.Group.Members);
    }
}