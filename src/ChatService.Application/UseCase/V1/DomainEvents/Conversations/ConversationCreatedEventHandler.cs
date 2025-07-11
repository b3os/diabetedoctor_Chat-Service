// namespace ChatService.Application.UseCase.V1.DomainEvents.Conversations;
//
// public class ConversationCreatedEventHandler(
//     IUnitOfWork unitOfWork,
//     IOptions<KafkaSettings> kafkaSettings,
//     IOutboxEventRepository outboxEventRepository)
//     : IDomainEventHandler<ConversationCreatedEvent>
// {
//     public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
//     {
//         var integrationEvent = MapToIntegrationEvent(notification);
//         var @event = OutboxEventExtension.ToOutboxEvent(kafkaSettings.Value.ConversationTopic, integrationEvent);
//         await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
//     }
//
//     private ConversationCreatedIntegrationEvent MapToIntegrationEvent(ConversationCreatedEvent notification)
//     {
//         return new ConversationCreatedIntegrationEvent
//         {
//             ConversationId = notification.ConversationId,
//             ConversationName = notification.ConversationName,
//             Members = notification.MemberIds
//         };
//     }
// }