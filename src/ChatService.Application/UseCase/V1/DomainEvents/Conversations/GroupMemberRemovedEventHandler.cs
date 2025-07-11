namespace ChatService.Application.UseCase.V1.DomainEvents.Conversations;

public sealed class GroupMemberRemovedEventHandler(
    IUnitOfWork unitOfWork,
    IOptions<KafkaSettings> kafkaSettings,
    IOutboxEventRepository outboxEventRepository)
    : INotificationHandler<GroupMemberRemovedEvent>
{
    public async Task Handle(GroupMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = MapToIntegrationEvent(notification);
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    private GroupMemberRemovedIntegrationEvent MapToIntegrationEvent(GroupMemberRemovedEvent notification)
    {
        return new GroupMemberRemovedIntegrationEvent
        {
            ConversationId = notification.ConversationId,
            MemberId = notification.MemberId
        };
    }
}