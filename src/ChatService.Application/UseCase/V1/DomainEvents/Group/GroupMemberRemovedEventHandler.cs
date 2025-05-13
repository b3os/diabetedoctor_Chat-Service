using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.GroupIntegrationEvents;
using ChatService.Contract.Services.Group.DomainEvents;

namespace ChatService.Application.UseCase.V1.DomainEvents.Group;

public class GroupMemberRemovedEventHandler(IEventPublisher publisher) : IDomainEventHandler<GroupMemberRemovedEvent>
{
    public async Task Handle(GroupMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupMemberRemovedIntegrationEvent()
        {
            GroupId = notification.GroupId,
            MemberId = notification.MemberId
        };
        await publisher.PublishAsync(KafkaTopicConstraints.GroupTopic, integrationEvent);
    }
}