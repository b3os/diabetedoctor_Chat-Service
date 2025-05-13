using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.GroupIntegrationEvents;
using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Group;

public class GroupDeletedEventHandler(IEventPublisher publisher) : IDomainEventHandler<GroupDeletedEvent>
{
    public async Task Handle(GroupDeletedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupDeletedIntegrationEvent()
        {
            GroupId = notification.GroupId,
        };
        await publisher.PublishAsync(KafkaTopicConstraints.GroupTopic, integrationEvent);
    }
}