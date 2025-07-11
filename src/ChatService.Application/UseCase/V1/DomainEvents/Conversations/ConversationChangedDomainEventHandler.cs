namespace ChatService.Application.UseCase.V1.DomainEvents.Conversations;

public sealed class ConversationChangedDomainEventHandler(
    IUnitOfWork unitOfWork,
    IOptions<KafkaSettings> kafkaSettings,
    IOutboxEventRepository outboxEventRepository)
    : IDomainEventHandler<ConversationCreatedEvent>, 
        IDomainEventHandler<ConversationUpdatedEvent>,
        IDomainEventHandler<ConversationDeletedEvent>
        
{
    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ConversationCreatedIntegrationEvent
        {
            ConversationId = notification.ConversationId,
            ConversationName = notification.ConversationName,
            Members = notification.MemberIds,
        };
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    public async Task Handle(ConversationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ConversationUpdatedIntegrationEvent
        {
            ConversationId = notification.ConversationId,
            ConversationName = notification.Name,
            OldAvatar = notification.OldAvatar,
        };
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    public async Task Handle(ConversationDeletedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ConversationDeletedIntegrationEvent{ConversationId = notification.ConversationId};
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    

}