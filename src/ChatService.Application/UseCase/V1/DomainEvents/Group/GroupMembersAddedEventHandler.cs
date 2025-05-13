using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.GroupIntegrationEvents;
using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Group;

public class GroupMembersAddedEventHandler(IEventPublisher publisher) : IDomainEventHandler<GroupMembersAddedEvent>
{
    public async Task Handle(GroupMembersAddedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupMembersAddedIntegrationEvent()
        {
            GroupId = notification.GroupId,
            Members = notification.Members
        };
        await publisher.PublishAsync(KafkaTopicConstraints.GroupTopic, integrationEvent);
    }
}