using ChatService.Application.Mapping;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.MessageIntegrationEvents;
using ChatService.Contract.Services.Message.Commands;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IOutboxEventRepository outboxEventRepository,
    IUnitOfWork unitOfWork,
    IAblyEventPublisher ablyEventPublisher)
    : ICommandHandler<CreateMessageCommand, Response>
{
    public async Task<Result<Response>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var conversationFoundResult = await GetConversationAsync(request, cancellationToken);
        if (conversationFoundResult.IsFailure)
        {
            return Result.Failure<Response>(conversationFoundResult.Error);
        }
        
        var participantResult = await GetSenderInfoHasPermissionAsync(request.ConversationId, request.UserId, cancellationToken);
        if (participantResult.IsFailure)
        {
            return Result.Failure<Response>(participantResult.Error);
        }
            
        var message = MapToMessage(request.ConversationId, participantResult.Value.UserId, request);
        var integrationEvent = MapToIntegrationEvent(conversationFoundResult.Value, participantResult.Value, message);
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ChatTopic, integrationEvent);
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        await ablyEventPublisher.PublishAsync(AblyTopicConstraints.GlobalChatChannel, AblyTopicConstraints.GlobalChatEvent, integrationEvent);
        return Result.Success(new Response(
            MessageMessage.CreateMessageSuccessfully.GetMessage().Code,
            MessageMessage.CreateMessageSuccessfully.GetMessage().Message));
    }
    
    private async Task<Result<Domain.Models.Conversation>> GetConversationAsync(CreateMessageCommand command, CancellationToken cancellationToken)
    {
        var conversationProjection = Builders<Domain.Models.Conversation>.Projection
            .Include(conversation => conversation.Avatar)
            .Include(conversation => conversation.Name)
            .Include(conversation => conversation.ConversationType);
        var conversation = await conversationRepository.FindSingleAsync(
            group => group.Id == command.ConversationId
                     && group.ConversationType == (ConversationTypeEnum)command.ConversationType,
            conversationProjection,
            cancellationToken);
        
        return conversation is null
            ? Result.Failure<Domain.Models.Conversation>(ConversationErrors.NotFound)
            : Result.Success(conversation);
    }
    
    private async Task<Result<Participant>> GetSenderInfoHasPermissionAsync(ObjectId conversationId, string participantId,
        CancellationToken cancellationToken)
    {
        var participant = await participantRepository.FindSingleAsync(
            p => p.ConversationId == conversationId && p.UserId.Id == participantId,
            cancellationToken: cancellationToken);
        
        return participant is null
            ? Result.Failure<Participant>(ConversationErrors.YouNotConversationParticipant)
            : Result.Success(participant);
    }

    private Domain.Models.Message MapToMessage(ObjectId groupId, UserId userId, CreateMessageCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var type = Mapper.MapMessageType(command.MessageType);
        return Domain.Models.Message.Create(id, groupId, userId, command.Content!, type);
    }

    // private ChatCreatedEvent MapToDomainEvent(ObjectId groupId, Domain.Models.User sender, Domain.Models.Message message)
    // {
    //     return new ChatCreatedEvent
    //     {
    //         SenderId = sender.UserId.Id,
    //         SenderFullName = sender.Fullname,
    //         SenderAvatar = sender.Avatar.PublicUrl,
    //         MessageId = message.Id.ToString(),
    //         MessageContent = message.Content,
    //         GroupId = groupId.ToString()
    //     };
    // }
    
    private MessageCreatedIntegrationEvent MapToIntegrationEvent(Domain.Models.Conversation conversation, Participant sender, Domain.Models.Message message)
    {
        return new MessageCreatedIntegrationEvent
        {
            Sender = new SenderInfo
                { SenderId = sender.UserId.Id, FullName = sender.FullName, Avatar = sender.Avatar.PublicUrl},
            Conversation = conversation.ConversationType switch
            {
                ConversationTypeEnum.Group =>new ConversationInfo
                {
                    ConversationId = conversation.Id.ToString(),
                    ConversationName = conversation.Name,
                    Avatar = conversation.Avatar.PublicUrl,
                    ConversationType = (int)ConversationTypeEnum.Group,
                },
                _ =>  new ConversationInfo
                {
                    ConversationId = conversation.Id.ToString(),
                    ConversationType = (int)conversation.ConversationType,
                }
            },
            MessageId = message.Id.ToString(),
            MessageContent = message.Content,
            MessageType = (int) message.Type,
            CreatedDate = sender.CreatedDate
        };
    }
}