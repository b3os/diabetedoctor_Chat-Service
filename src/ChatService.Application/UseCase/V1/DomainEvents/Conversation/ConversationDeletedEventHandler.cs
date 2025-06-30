
namespace ChatService.Application.UseCase.V1.DomainEvents.Conversation;

public class ConversationDeletedEventHandler(
    IUnitOfWork unitOfWork,
    IOptions<KafkaSettings> kafkaSettings,
    IOutboxEventRepository outboxEventRepository)
    : IDomainEventHandler<ConversationDeletedEvent>
{
    public async Task Handle(ConversationDeletedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = MapToIntegrationEvent(notification);
        var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
        await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
    }
    
    private ConversationDeletedIntegrationEvent MapToIntegrationEvent(ConversationDeletedEvent notification)
    {
        return new ConversationDeletedIntegrationEvent{ConversationId = notification.ConversationId};
    }
}