namespace ChatService.Application.UseCase.V1.DomainEvents.Conversations;

public sealed class GroupMemberAddedEventHandler(
    IUnitOfWork unitOfWork,
    IOptions<KafkaSettings> kafkaSettings,
    IOutboxEventRepository outboxEventRepository)
    : IDomainEventHandler<GroupMembersAddedEvent>
{
    public async Task Handle(GroupMembersAddedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = MapToIntegrationEvent(notification);
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    private GroupMembersAddedIntegrationEvent MapToIntegrationEvent(GroupMembersAddedEvent notification)
    {
        return new GroupMembersAddedIntegrationEvent(notification.ConversationId, notification.Members);
    }
}