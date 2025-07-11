// namespace ChatService.Application.UseCase.V1.DomainEvents.Conversations;
//
// public class ConversationUpdatedEventHandler(
//     IUnitOfWork unitOfWork,
//     IOptions<KafkaSettings> kafkaSettings,
//     IOutboxEventRepository outboxEventRepository)
//     : IDomainEventHandler<ConversationUpdatedEvent>
// {
//     public async Task Handle(ConversationUpdatedEvent notification, CancellationToken cancellationToken)
//     {
//         var integrationEvent = MapToIntegrationEvent(notification);
//         var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
//         await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
//     }
//     
//     private ConversationUpdatedIntegrationEvent MapToIntegrationEvent(ConversationUpdatedEvent notification)
//     {
//         return new ConversationUpdatedIntegrationEvent
//         {
//             ConversationId = notification.ConversationId,
//             ConversationName = notification.Name,
//             OldAvatar = notification.OldAvatar,
//         };
//     }
// }