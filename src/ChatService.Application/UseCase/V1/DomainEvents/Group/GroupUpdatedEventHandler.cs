using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.GroupIntegrationEvents;
using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Group;

public class GroupUpdatedEventHandler(IEventPublisher publisher) : IDomainEventHandler<GroupUpdatedEvent>
{
    public async Task Handle(GroupUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupUpdatedIntegrationEvent()
        {
            GroupId = notification.GroupId,
            Name = notification.Name,
            Avatar = notification.Avatar
        };
        await publisher.PublishAsync(KafkaTopicConstraints.GroupTopic, integrationEvent);
    }
}