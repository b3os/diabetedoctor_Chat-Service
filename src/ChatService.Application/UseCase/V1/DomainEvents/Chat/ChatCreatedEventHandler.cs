using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.ChatIntegrationEvents;
using ChatService.Contract.Services.Message.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Chat;

public class ChatCreatedEventHandler(IEventPublisher publisher) : IDomainEventHandler<ChatCreatedEvent>
{
    public async Task Handle(ChatCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ChatCreatedIntegrationEvent()
        {
            GroupId = notification.GroupId,
            MessageId = notification.MessageId,
            MessageContent = notification.MessageContent,
            SenderId = notification.SenderId
        };
        await publisher.PublishAsync(KafkaTopicConstraints.ChatTopic, integrationEvent);
    }
}