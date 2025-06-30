using ChatService.Application.Mapping;
using ChatService.Contract.DTOs.ConversationDtos;
using ChatService.Contract.DTOs.EnumDtos;
using ChatService.Contract.Enums;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.MessageIntegrationEvents;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Message.Commands;
using CloudinaryDotNet.Actions;
using MongoDB.Bson.Serialization;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository,
    IMediaRepository mediaRepository,
    IOutboxEventRepository outboxEventRepository,
    IUnitOfWork unitOfWork,
    IAblyEventPublisher ablyEventPublisher)
    : ICommandHandler<CreateMessageCommand, Response>
{
    public async Task<Result<Response>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await GetConversationWithParticipantAsync(request, cancellationToken);
        if (conversation.IsFailure)
        {
            return Result.Failure<Response>(conversation.Error);
        }

        var id = ObjectId.GenerateNewId();
        var userId = Mapper.MapUserId(conversation.Value.Member!.User.UserId);

        Domain.Models.Media? media = null;
        Domain.Models.Message message;
        switch (request.MessageType)
        {
            case MessageTypeEnum.Text:
                message = Domain.Models.Message.CreateText(id, request.ConversationId, userId, request.Content!);
                break;
            case MessageTypeEnum.File:
                var mediaId = ObjectId.Parse(request.MediaId);
                media = await mediaRepository.FindByIdAsync(mediaId, cancellationToken);
                if (media is null)
                {
                    return Result.Failure<Response>(MediaErrors.MediaNotFound);
                }

                var file = FileAttachment.Of(media.PublicId, media.PublicUrl, media.MediaType);
                message = Domain.Models.Message.CreateFile(id, request.ConversationId, userId, media.OriginalFileName,
                    file);
                media.Use();
                break;
            default:
                throw new NotSupportedException($"Unsupported message type: {request.MessageType.ToString()}");
        }

        var integrationEvent = MapToIntegrationEvent(conversation.Value, message);

        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);
            if (media is not null)
            {
                await mediaRepository.ReplaceOneAsync(unitOfWork.ClientSession, media, cancellationToken);
            }
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ChatTopic, integrationEvent);
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        await ablyEventPublisher.PublishAsync(AblyTopicConstraints.GlobalChatChannel,
            AblyTopicConstraints.GlobalChatEvent, integrationEvent);
        return Result.Success(new Response(
            MessageMessage.CreateMessageSuccessfully.GetMessage().Code,
            MessageMessage.CreateMessageSuccessfully.GetMessage().Message));
    }

    private async Task<Result<ConversationWithParticipantDto>> GetConversationWithParticipantAsync(
        CreateMessageCommand request,
        CancellationToken cancellationToken)
    {
        var document = await conversationRepository.GetConversationWithParticipant(request.ConversationId,
            request.UserId, (ConversationType)request.ConversationType, cancellationToken);

        if (document is null)
        {
            return Result.Failure<ConversationWithParticipantDto>(ConversationErrors.NotFound);
        }

        var conversation = BsonSerializer.Deserialize<ConversationWithParticipantDto>(document);

        if (conversation.Member is null)
        {
            return Result.Failure<ConversationWithParticipantDto>(ConversationErrors.NotFound);
        }

        return conversation.Status is ConversationStatusEnum.Closed 
            ? Result.Failure<ConversationWithParticipantDto>(ConversationErrors.ThisConversationIsClosed) 
            : Result.Success(conversation);
    }

    private MessageCreatedIntegrationEvent MapToIntegrationEvent(ConversationWithParticipantDto conversation,
        Domain.Models.Message message)
    {
        return new MessageCreatedIntegrationEvent
        {
            Sender = new SenderInfo
            {
                SenderId = conversation.Member!.User.UserId.Id,
                FullName = conversation.Member!.User.FullName,
                Avatar = conversation.Member!.User.Avatar.PublicUrl
            },
            Conversation = conversation.ConversationType switch
            {
                ConversationTypeEnum.Group => new ConversationInfo
                {
                    ConversationId = conversation.Id,
                    ConversationName = conversation.Name,
                    Avatar = conversation.Avatar.PublicUrl,
                    ConversationType = (int)ConversationType.Group
                },
                _ => new ConversationInfo
                {
                    ConversationId = conversation.Id,
                    ConversationType = (int)conversation.ConversationType
                }
            },
            MessageId = message.Id.ToString(),
            MessageContent = message.Content,
            MessageType = (int)message.Type,
            CreatedDate = message.CreatedDate
        };
    }
}