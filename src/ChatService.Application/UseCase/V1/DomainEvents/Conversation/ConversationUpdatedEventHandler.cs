// using ChatService.Contract.Common.Constraint;
// using ChatService.Contract.EventBus.Abstractions;
// using ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;
// using ChatService.Contract.Services.Group.DomainEvents;
//
// namespace ChatService.Application.UseCase.V1.DomainEvents.Conversation;
//
// public class ConversationUpdatedEventHandler(IEventPublisher publisher) : IDomainEventHandler<ConversationUpdatedEvent>
// {
//     public async Task Handle(ConversationUpdatedEvent notification, CancellationToken cancellationToken)
//     {
//         var integrationEvent = new ConversationUpdatedIntegrationEvent()
//         {
//             ConversationId = notification.ConversationId,
//             ConversationName = notification.Name,
//             Avatar = notification.Avatar
//         };
//         await publisher.PublishAsync(KafkaTopicConstraints.ConversationTopic, integrationEvent);
//     }
// }