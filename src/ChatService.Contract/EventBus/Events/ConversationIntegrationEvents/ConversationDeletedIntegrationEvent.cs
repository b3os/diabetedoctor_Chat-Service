namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record ConversationDeletedIntegrationEvent : IntegrationEvent
{
    public string ConversationId { get; init; } = null!;
}