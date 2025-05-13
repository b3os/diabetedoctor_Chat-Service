using ChatService.Application.Mapping;
using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.ChatIntegrationEvents;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Message;
using ChatService.Contract.Services.Message.DomainEvents;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MediatR;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : ICommandHandler<CreateMessageCommand>
{
    public async Task<Result> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        
        await EnsureUserExists(request.UserId, cancellationToken);
        
        await EnsureAccessGroupPermissionAsync(request.GroupId, request.UserId, cancellationToken);
        
        var message = MapToMessage(request.GroupId, request.UserId, request);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);
            
            await publisher.Publish(MapToEvent(request.GroupId, request.UserId, message), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(MessageMessage.CreateMessageSuccessfully.GetMessage().Code,
            MessageMessage.CreateMessageSuccessfully.GetMessage().Message));
    }

    private async Task EnsureUserExists(string userId, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsAsync(user => user.UserId.Id == userId,
            cancellationToken: cancellationToken);
        
        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }
    
    private async Task EnsureAccessGroupPermissionAsync(ObjectId groupId, string userId, CancellationToken cancellationToken)
    {
        var exists = await groupRepository.ExistsAsync(
            group => group.Id == groupId && group.Members.Any(member => member.UserId.Id == userId),
            cancellationToken: cancellationToken);
        
        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }

    private Domain.Models.Message MapToMessage(ObjectId groupId, string userId, CreateMessageCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var senderUserId = UserId.Of(userId);
        var type = Mapper.MapMessageType(command.Type);
        return Domain.Models.Message.Create(id, groupId, senderUserId, command.Content!, type);
    }

    private ChatCreatedEvent MapToEvent(ObjectId groupId, string senderId, Domain.Models.Message message)
    {
        return new ChatCreatedEvent
        {
            MessageId = message.Id.ToString(),
            MessageContent = message.Content,
            SenderId = senderId,
            GroupId = groupId.ToString(),
        };
    }
}