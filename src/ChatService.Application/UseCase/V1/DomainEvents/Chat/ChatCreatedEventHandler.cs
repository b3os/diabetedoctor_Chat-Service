using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.ChatIntegrationEvents;
using ChatService.Contract.Services.Message.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Chat;

public class ChatCreatedEventHandler(IEventPublisher publisher, IAblyEventPublisher ablyEventPublisher)
    : IDomainEventHandler<ChatCreatedEvent>
{
    public async Task Handle(ChatCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ChatCreatedIntegrationEvent()
        {
            Sender = new SenderInfo()
                { SenderId = notification.SenderId, FullName = notification.SenderFullName, Avatar = notification.SenderAvatar },
            GroupId = notification.GroupId,
            MessageId = notification.MessageId,
            MessageContent = notification.MessageContent,
            Type = notification.Type,
            ReadBy = notification.ReadBy,
            CreatedDate = notification.CreatedDate
        };
        await ablyEventPublisher.PublishAsync(AblyTopicConstraints.GlobalChatChannel,
            AblyTopicConstraints.GlobalChatEvent, integrationEvent);
        await publisher.PublishAsync(KafkaTopicConstraints.ChatTopic, integrationEvent);
    }
}