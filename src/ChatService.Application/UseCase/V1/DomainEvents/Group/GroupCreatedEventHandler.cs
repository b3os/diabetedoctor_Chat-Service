using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.GroupIntegrationEvents;
using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Group;

public class GroupCreatedEventHandler(IEventPublisher publisher) : IDomainEventHandler<GroupCreatedEvent>
{
    public async Task Handle(GroupCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupCreatedIntegrationEvent(){GroupId = notification.GroupId, Name = notification.Name, Members = notification.Members};
        await publisher.PublishAsync(KafkaTopicConstraints.GroupTopic, integrationEvent);
    }
}